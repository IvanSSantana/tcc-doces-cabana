using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    // Com os itens (e o produto de cada um) e o endereço de entrega
    // incluídos — BuscarPorId (do IRepository genérico) não traz nada disso,
    // e tanto a confirmação (spec 022) quanto o detalhe (spec 023, RF-06 a
    // RF-09) precisam. Sem repositório para item nem para pagamento: Pedido
    // é a raiz do agregado, grava e lê os itens junto (spec 022, plano §3).
    //
    // Sem BuscarPorId(pedidoId) sozinho de propósito (spec 023, plano §1) —
    // mesmo desenho de IEnderecoRepository.Buscar: a busca já filtra pelo
    // par pedido-e-dono, então RN-01 não pode ser violada por esquecimento,
    // não depende de checagem posterior.
    Task<Pedido?> Buscar(Guid pedidoId, Guid usuarioId);

    Task<List<Pedido>> ListarPorUsuario(Guid usuarioId);

    // Pagamento não tem repositório próprio (plano §3) — é lido/gravado por
    // aqui, junto com o pedido que o possui.
    Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId);

    // Adiciona pedido (com os itens, via navegação) e pagamento juntos, sem
    // persistir — quem chama decide quando `IUnitOfWork.SalvarAlteracoes` roda,
    // e é essa única chamada que entrega a RN-07 (Princípio VI).
    Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento);
}
