using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> Buscar(Guid pedidoId, Guid usuarioId);

    Task<List<Pedido>> ListarPorUsuario(Guid usuarioId);

    Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId);

    Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento);
}
