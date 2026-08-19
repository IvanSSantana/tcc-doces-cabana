using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<List<Categoria>> BuscarTodasComSubcategorias() =>
        await _context.Categorias
            .Include(c => c.Subcategorias)
            .AsNoTracking()
            .ToListAsync();
}
