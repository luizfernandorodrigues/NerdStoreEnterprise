using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials.Interfaces;
using NSE.Core.Messages.Integration;
using NSE.Identidade.API.Models;
using NSE.MessageBus;
using NSE.WebAPI.Core.Controllers;
using NSE.WebAPI.Core.Identidade;
using NSE.WebAPI.Core.Usuario;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NSE.Identidade.API.Controllers
{
    [Route("api/identidade")]
    public class AuthController : MainController

    {
        #region Atributos

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IMessageBus _bus;
        private readonly IAspNetUser _aspNetUser;
        private readonly IJsonWebKeySetService _jksService;

        #endregion

        #region Construtor

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppSettings> appSettings, IMessageBus bus, IAspNetUser aspNetUser, IJsonWebKeySetService jksService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _bus = bus;
            _aspNetUser = aspNetUser;
            _jksService = jksService;
        }

        #endregion

        #region Controllers

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha);

            if (result.Succeeded)
            {
                var clientResult = await RegistrarCliente(usuarioRegistro);

                if (!clientResult.ValidationResult.IsValid)
                {
                    await _userManager.DeleteAsync(user);

                    return CustomResponse(clientResult.ValidationResult);
                }

                return CustomResponse(await GerarJwt(usuarioRegistro.Email));
            }

            foreach (var error in result.Errors)
                AdicionarErroProcessamento(error.Description);

            return CustomResponse();
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, false, true);

            if (result.Succeeded)
                return CustomResponse(await GerarJwt(usuarioLogin.Email));

            if (result.IsLockedOut)
            {
                AdicionarErroProcessamento("Usuário temporariamente bloqueado por tentativas inválidas");

                return CustomResponse();
            }

            AdicionarErroProcessamento("Usuário ou Senha incorretos");

            return CustomResponse();
        }

        #endregion

        #region Métodos Privados

        private async Task<ResponseMessage> RegistrarCliente(UsuarioRegistro usuarioRegistro)
        {
            var usuario = await _userManager.FindByEmailAsync(usuarioRegistro.Email);

            var usuarioRegistrado = new UsuarioRegistradoIntegrationEvent(Guid.Parse(usuario.Id), usuarioRegistro.Nome, usuarioRegistro.Email, usuarioRegistro.Cpf);

            try
            {
                return await _bus.RequestAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(usuarioRegistrado);
            }
            catch
            {
                await _userManager.DeleteAsync(usuario);

                throw;
            }
        }


        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var claims = await _userManager.GetClaimsAsync(user);

            var identityClaims = await ObterClaimsUsuario(claims, user);

            var encodedToken = CodificarToken(identityClaims);

            return ObterRespostaToken(encodedToken, user, claims);
        }

        private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, IdentityUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in userRoles)
                claims.Add(new Claim("role", userRole));

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private string CodificarToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var currentIssuer = $"{_aspNetUser.ObterHttpContext().Request.Scheme}://{_aspNetUser.ObterHttpContext().Request.Host}";

            var key = _jksService.GetCurrent();

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = currentIssuer,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = key
            });

            var encodedToken = tokenHandler.WriteToken(token);

            return encodedToken;
        }

        private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            var response = new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UsuarioToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        #endregion
    }
}
