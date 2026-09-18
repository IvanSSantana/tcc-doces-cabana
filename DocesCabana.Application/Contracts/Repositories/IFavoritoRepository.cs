using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

// Não estende IRepository<T>: a chave de Favorito é composta (ProdutoId, UsuarioId)
public interface IFavoritoRepository
{
    Task<List<Favorito>> BuscarPorUsuario(Guid usuarioId);

    Task<Favorito?> Buscar(Guid produtoId, Guid usuarioId);

    Task<HashSet<Guid>> IdsPorUsuario(Guid usuarioId, IEnumerable<Guid> produtoIds);

    Task Adicionar(Favorito favorito);

    void Remover(Favorito favorito);
}
