using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> Buscar(Guid pedidoId, Guid usuarioId);

    Task<List<Pedido>> ListarPorUsuario(Guid usuarioId);

    Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId);

    Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento);

    Task<bool> ExisteEntregueComProduto(Guid usuarioId, Guid produtoId);

    Task<bool> ClienteJaUsouCupom(Guid usuarioId, Guid cupomId);

    Task<Dictionary<Guid, int>> ContarUsosPorCupom();

    /// <summary>Pedido rastreado, sem itens, para mudar a situação. Nulo se não for do usuário.</summary>
    Task<Pedido?> BuscarParaAlterar(Guid pedidoId, Guid usuarioId);

    /// <summary>Pedidos que contam como receita (confirmado, enviado, entregue) com itens e produto, no intervalo UTC [inicio, fim).</summary>
    Task<List<Pedido>> ListarReceitaEntre(DateTime inicioUtc, DateTime fimUtc);
}
