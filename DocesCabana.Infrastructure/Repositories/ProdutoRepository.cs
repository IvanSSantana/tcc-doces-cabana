using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<Produto?> BuscarDetalhePorId(Guid id) =>
        await _context.Produtos
            .Include(p => p.Subcategoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProdutoId == id);
}
