using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;

namespace DocesCabana.Infrastructure.Repositories;

public class SubcategoriaRepository : Repository<Subcategoria>, ISubcategoriaRepository
{
    public SubcategoriaRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }
}
