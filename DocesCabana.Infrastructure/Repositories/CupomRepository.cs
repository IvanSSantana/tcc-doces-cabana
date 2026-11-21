using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class CupomRepository : Repository<Cupom>, ICupomRepository
{
    public CupomRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<Cupom?> BuscarPorCodigo(string codigoNormalizado) =>
        await _context.Cupons
            .Include(c => c.Categoria)
            .Include(c => c.Subcategoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == codigoNormalizado);

    public async Task<bool> ExisteCodigo(string codigoNormalizado) =>
        await _context.Cupons
            .AsNoTracking()
            .AnyAsync(c => c.Codigo == codigoNormalizado);

    public async Task<List<Cupom>> ListarParaGestao() =>
        await _context.Cupons
            .Include(c => c.Categoria)
            .Include(c => c.Subcategoria)
            .AsNoTracking()
            .OrderByDescending(c => c.DataInicio)
            .ThenBy(c => c.Codigo)
            .ToListAsync();
}
