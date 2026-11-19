using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class PromocaoRepository : Repository<Promocao>, IPromocaoRepository
{
    public PromocaoRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<List<Promocao>> ListarVigentes(DateOnly hoje) =>
        await _context.Promocoes
            .Where(p => p.Ativa && p.DataInicio <= hoje && p.DataFim >= hoje)
            .Include(p => p.Produtos)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Promocao>> ListarParaGestao() =>
        await _context.Promocoes
            .Include(p => p.Categoria)
            .Include(p => p.Subcategoria)
            .Include(p => p.Produtos)
            .AsSplitQuery()
            .AsNoTracking()
            .OrderByDescending(p => p.DataInicio)
            .ThenBy(p => p.Nome)
            .ToListAsync();

    public async Task<Promocao?> BuscarComProdutos(Guid promocaoId) =>
        await _context.Promocoes
            .Include(p => p.Produtos)
            .FirstOrDefaultAsync(p => p.PromocaoId == promocaoId);
}
