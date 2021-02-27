using Dapper;
using NSE.Pedidos.API.Application.DTO;
using NSE.Pedidos.Domain.Pedidos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSE.Pedidos.API.Application.Queries
{
    public interface IPedidoQueries
    {
        Task<PedidoDTO> ObterUltimoPedido(Guid clienteId);
        Task<IEnumerable<PedidoDTO>> ObterListaPorCliente(Guid guid);
    }

    public class PedidoQueries : IPedidoQueries
    {
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoQueries(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<PedidoDTO> ObterUltimoPedido(Guid clienteId)
        {
            const string sql = @"SELECT 
	                                PED.Id AS 'ProdutoId',
	                                PED.Codigo,
	                                PED.VoucherUtilizado,
	                                PED.Desconto,
	                                PED.ValorTotal,
	                                PED.PedidoStatus,
	                                PED.Logradouro,
	                                PED.Numero,
	                                PED.Bairro,
	                                PED.Cep,
	                                PED.Complemento,
	                                PED.Cidade,
	                                PED.Estado,
	                                PED2.Id AS 'ProdutoItemId',
	                                PED2.ProdutoNome,
	                                PED2.Quantidade,
	                                PED2.ProdutoImagem,
	                                PED2.ValorUnitario
                                FROM 
	                                dbo.Pedidos PED 
	                                INNER JOIN dbo.PedidoItems PED2 ON PED.Id = PED2.PedidoId 
                                WHERE 
	                                PED.ClienteId = @clienteId AND 
	                                PED.DataCadastro BETWEEN DATEADD(MINUTE, -3 GETDATE()) AND DATEADD(MINUTE, 0, GETDATE()) AND 
	                                PED.PedidoStatus = 1 
                                ORDER BY 
	                                PED.DataCadastro DESC";

            var pedido = await _pedidoRepository.ObterConexao().QueryAsync<dynamic>(sql, new { clienteId });

            return MapearPedido(pedido);
        }

        public async Task<IEnumerable<PedidoDTO>> ObterListaPorCliente(Guid clienteId)
        {
            var pedidos = await _pedidoRepository.ObterListaPorClienteId(clienteId);

            return pedidos.Select(PedidoDTO.ParaPedidoDTO);
        }

        private PedidoDTO MapearPedido(dynamic result)
        {
            var pedido = new PedidoDTO
            {
                Codigo = result[0].Codigo,
                Status = result[0].PedidoStatus,
                ValorTotal = result[0].ValorTotal,
                Desconto = result[0].Desconto,
                VoucherUtilizado = result[0].VoucherUtilizado,

                PedidoItems = new List<PedidoItemDTO>(),
                Endereco = new EnderecoDTO
                {
                    Logradouro = result[0].Logradouro,
                    Bairro = result[0].Bairro,
                    Cep = result[0].Cep,
                    Cidade = result[0].Cidade,
                    Complemento = result[0].Complemento,
                    Estado = result[0].Estado,
                    Numero = result[0].Numero
                }
            };

            foreach(var item in result)
            {
                var pedidoItem = new PedidoItemDTO
                {
                    Nome = item.ProdutoNome,
                    Valor = item.ValorUnitario,
                    Quantidade = item.Quantidade,
                    Imagem = item.ProdutoImagem
                };

                pedido.PedidoItems.Add(pedidoItem);
            }

            return pedido;
        }
    }
}
