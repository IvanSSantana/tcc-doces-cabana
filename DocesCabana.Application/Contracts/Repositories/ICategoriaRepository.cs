using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<List<Categoria>> BuscarTodasComSubcategorias();
}
