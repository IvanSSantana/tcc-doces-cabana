using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

// Não estende IRepository<T>: a chave de Favorito é composta
// (ProdutoId, UsuarioId), e IRepository<T>.BuscarPorId(Guid) assume chave
// simples — o mesmo motivo pelo qual VotoUtil nunca teve repositório próprio
// (spec 015, plano §1).
public interface IFavoritoRepository
{
    Task<List<Favorito>> BuscarPorUsuario(Guid usuarioId);

    Task<Favorito?> Buscar(Guid produtoId, Guid usuarioId);

    // Devolve, dentre os identificadores recebidos, só os que o usuário
    // favoritou — uma consulta por página do catálogo, não uma por cartão
    // (spec 015, RF-02, plano §5).
    Task<HashSet<Guid>> IdsPorUsuario(Guid usuarioId, IEnumerable<Guid> produtoIds);

    Task Adicionar(Favorito favorito);

    void Remover(Favorito favorito);
}
