using FluentValidation.Results;
using MediatR;
using NSE.Core.Messages;
using NSE.Core.Messages.Integration;
using NSE.MessageBus;
using NSE.Pedidos.API.Application.DTO;
using NSE.Pedidos.API.Application.Events;
using NSE.Pedidos.Domain.Pedidos;
using NSE.Pedidos.Domain.Vouchers;
using NSE.Pedidos.Domain.Vouchers.Specs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSE.Pedidos.API.Application.Commands
{
    public class PedidoCommandHandler : CommandHandler, IRequestHandler<AdicionarPedidoCommand, ValidationResult>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMessageBus _bus;

        public PedidoCommandHandler(IVoucherRepository voucherRepository, IPedidoRepository pedidoRepository, IMessageBus bus)
        {
            _voucherRepository = voucherRepository;
            _pedidoRepository = pedidoRepository;
            _bus = bus;
        }

        public async Task<ValidationResult> Handle(AdicionarPedidoCommand message, CancellationToken cancellationToken)
        {
            // validar comando
            if (!message.EhValido())
                return message.ValidationResult;

            //mapear pedido
            var pedido = MapearPedido(message);

            // aplicar voucher
            if (!await AplicarVoucher(message, pedido))
                return ValidationResult;

            // validar pedido
            if (!ValidarPedido(pedido))
                return ValidationResult;

            // processar pagamento
            if (!await ProcessarPagamento(pedido, message))
                return ValidationResult;

            // se pagamento ok
            pedido.AutorizarPedido();

            // adicionar evento
            pedido.AdicionarEvento(new PedidoRealizadoEvent(pedido.Id, pedido.ClienteId));

            // adicionar pedido repositorio
            _pedidoRepository.Adicionar(pedido);

            // persistir dados do pedido e voucher
            return await PersistirDados(_pedidoRepository.UnitOfWork);
        }

        private Pedido MapearPedido(AdicionarPedidoCommand message)
        {
            var endereco = new Endereco
            {
                Logradouro = message.Endereco.Logradouro,
                Numero = message.Endereco.Numero,
                Complemento = message.Endereco.Complemento,
                Bairro = message.Endereco.Bairro,
                Cep = message.Endereco.Cep,
                Cidade = message.Endereco.Cidade,
                Estado = message.Endereco.Estado
            };

            var pedido = new Pedido(message.ClienteId, message.ValorTotal, message.PedidoItems.Select(PedidoItemDTO.ParaPedidoItem).ToList(), message.VoucherUtilizado, message.Desconto);

            pedido.AtribuirEndereco(endereco);

            return pedido;
        }
        private async Task<bool> AplicarVoucher(AdicionarPedidoCommand message, Pedido pedido)
        {
            if (!message.VoucherUtilizado)
                return true;

            var voucher = await _voucherRepository.ObterVoucherPorCodigo(message.VoucherCodigo);

            if (voucher == null)
            {
                AdicionarErro("O voucher informado não existe!");

                return false;
            }

            var voucherValidation = new VoucherValidation().Validate(voucher);

            if (!voucherValidation.IsValid)
            {
                voucherValidation.Errors.ToList().ForEach(m => AdicionarErro(m.ErrorMessage));

                return false;
            }

            pedido.AtribuirVoucher(voucher);
            voucher.DebitarQuantidade();

            _voucherRepository.Atualizar(voucher);

            return true;
        }
        private bool ValidarPedido(Pedido pedido)
        {
            var pedidoValorOriginal = pedido.ValorTotal;
            var pedidoDesconto = pedido.Desconto;

            pedido.CalcularValorPedido();

            if (pedido.ValorTotal != pedidoValorOriginal)
            {
                AdicionarErro("O valor total do pedido não confere com o cálculo do pedido");

                return false;
            }

            if (pedido.Desconto != pedidoDesconto)
            {
                AdicionarErro("O valor total não confere com o cálculo do pedido");

                return false;
            }

            return true;
        }
        public async Task<bool> ProcessarPagamento(Pedido pedido, AdicionarPedidoCommand message)
        {
            var pedidoIniciado = new PedidoIniciadoIntegrationEvent
            {
                PedidoId = pedido.Id,
                ClienteId = pedido.ClienteId,
                Valor = pedido.ValorTotal,
                TipoPagamento = 1, // fixo. Alterar se tiver mais tipos
                NomeCartao = message.NomeCartao,
                NumeroCartao = message.NumeroCartao,
                MesAnoVencimento = message.ExpiracaoCartao,
                CVV = message.CvvCartao
            };

            var result = await _bus.RequestAsync<PedidoIniciadoIntegrationEvent, ResponseMessage>(pedidoIniciado);

            if (result.ValidationResult.IsValid)
                return true;

            foreach (var erro in result.ValidationResult.Errors)
                AdicionarErro(erro.ErrorMessage);

            return false;
        }
    }
}
