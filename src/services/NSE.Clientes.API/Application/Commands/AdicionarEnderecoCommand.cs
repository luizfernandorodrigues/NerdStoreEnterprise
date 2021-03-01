using FluentValidation;
using NSE.Core.Messages;
using System;

namespace NSE.Clientes.API.Application.Commands
{
    public class AdicionarEnderecoCommand : Command
    {
        public Guid ClienteId { get; set; }
        public string Logradouro { get; set; }
        public string  Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cep { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }

        public AdicionarEnderecoCommand() { }

        public AdicionarEnderecoCommand(Guid clienteId, string logradouro, string numero, string complemento, string bairro, string cep, string cidade, string estado)
        {
            ClienteId = clienteId;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cep = cep;
            Cidade = cidade;
            Estado = estado;
        }

        public override bool EhValido()
        {
            ValidationResult = new EnderecoValidation().Validate(this);

            return ValidationResult.IsValid;
        }

        public class EnderecoValidation : AbstractValidator<AdicionarEnderecoCommand>
        {
            public EnderecoValidation()
            {
                RuleFor(x => x.Logradouro).NotEmpty().WithMessage("Informe o Logradouro");
                RuleFor(x => x.Numero).NotEmpty().WithMessage("Informe o Número");
                RuleFor(x => x.Complemento).NotEmpty().WithMessage("Informe o Complemento");
                RuleFor(x => x.Bairro).NotEmpty().WithMessage("Informe o Bairro");
                RuleFor(x => x.Cep).NotEmpty().WithMessage("Informe o Cep");
                RuleFor(x => x.Cidade).NotEmpty().WithMessage("Informe a Cidade");
                RuleFor(x => x.Estado).NotEmpty().WithMessage("Informe o Estado");
            }
        }
    }
}
