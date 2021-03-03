using NSE.Core.Messages.Integration;
using NSE.Pagamentos.API.Models;
using System;
using System.Threading.Tasks;

namespace NSE.Pagamentos.API.Services
{
    public interface IPagamentoService
    {
        Task<ResponseMessage> AutorizarPagamento(Pagamento pagamento);
        Task<ResponseMessage> CapturarPagamento(Guid pedidoId);
        Task<ResponseMessage> CancelarPagamento(Guid pedidoId);
    }
}