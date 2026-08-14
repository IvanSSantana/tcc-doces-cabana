using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    // BuscarPorId (do IRepository genérico) não traz a subcategoria — a
    // página do produto precisa do nome dela no caminho de navegação (RF-02).
    Task<Produto?> BuscarDetalhePorId(Guid id);
}
