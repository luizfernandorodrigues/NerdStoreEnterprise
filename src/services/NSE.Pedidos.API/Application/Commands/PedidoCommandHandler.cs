using FluentValidation.Results;
using MediatR;
using NSE.Core.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace NSE.Pedidos.API.Application.Commands
{
    public class PedidoCommandHandler : CommandHandler, IRequestHandler<AdicionarPedidoCommand, ValidationResult>
    {
        public Task<ValidationResult> Handle(AdicionarPedidoCommand message, CancellationToken cancellationToken)
        {
            // validar comando

            //mapear pedido

            // aplicar voucher

            // validar pedido

            // processar pagamento

            // se pagamento ok

            // adicionar evento

            // adicionar pedido repositorio

            // persistir dados do pedido e voucher
        }
    }
}
