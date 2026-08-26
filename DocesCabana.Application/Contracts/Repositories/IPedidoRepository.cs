using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    // Com os itens incluídos — BuscarPorId (do IRepository genérico) não
    // traz a coleção, e a confirmação do pedido (RF-22) precisa dela. Sem
    // repositório para item nem para pagamento: Pedido é a raiz do agregado,
    // grava e lê os itens junto (spec 022, plano §3).
    Task<Pedido?> BuscarPorIdComItens(Guid pedidoId);

    Task<List<Pedido>> ListarPorUsuario(Guid usuarioId);

    // Pagamento não tem repositório próprio (plano §3) — é lido/gravado por
    // aqui, junto com o pedido que o possui.
    Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId);

    // Adiciona pedido (com os itens, via navegação) e pagamento juntos, sem
    // persistir — quem chama decide quando `IUnitOfWork.SalvarAlteracoes` roda,
    // e é essa única chamada que entrega a RN-07 (Princípio VI).
    Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento);
}
