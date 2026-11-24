using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class BannerRepository : Repository<Banner>, IBannerRepository
{
    public BannerRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<List<Banner>> ListarVigentes(DateOnly hoje) =>
        await ComDestino()
            .Where(b => b.Ativo && b.DataInicio <= hoje && (b.DataFim == null || b.DataFim >= hoje))
            .Where(b => b.ProdutoId == null || b.Produto!.Status != ProdutoStatus.Inativo)
            .AsNoTracking()
            .OrderBy(b => b.Ordem)
            .ThenBy(b => b.DataInicio)
            .ToListAsync();

    public async Task<List<Banner>> ListarParaGestao() =>
        await ComDestino()
            .AsNoTracking()
            .OrderBy(b => b.Ordem)
            .ThenByDescending(b => b.DataInicio)
            .ToListAsync();

    private IQueryable<Banner> ComDestino() =>
        _context.Banners
            .Include(b => b.Categoria)
            .Include(b => b.Subcategoria)
                .ThenInclude(s => s!.Categoria)
            .Include(b => b.Produto);
}
