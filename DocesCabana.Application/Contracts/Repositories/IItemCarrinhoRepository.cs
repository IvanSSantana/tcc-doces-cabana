using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IItemCarrinhoRepository
{
    Task<List<ItemCarrinho>> BuscarPorUsuario(Guid usuarioId);

    Task<ItemCarrinho?> Buscar(Guid usuarioId, Guid produtoId);

    Task<int> ContarItens(Guid usuarioId);

    Task Adicionar(ItemCarrinho item);

    void Remover(ItemCarrinho item);
}
