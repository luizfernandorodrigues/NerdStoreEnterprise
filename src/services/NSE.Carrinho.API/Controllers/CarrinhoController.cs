using Microsoft.AspNetCore.Authorization;
using NSE.WebAPI.Core.Controllers;

namespace NSE.Carrinho.API.Controllers
{
    [Authorize]
    public class CarrinhoController : MainController
    {
    }
}
