using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly DocesCabanaDbContext _context;

    public FavoritoRepository(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Favorito>> BuscarPorUsuario(Guid usuarioId) =>
        await _context.Favoritos
            .AsNoTracking()
            .Include(f => f.Produto)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task<Favorito?> Buscar(Guid produtoId, Guid usuarioId) =>
        await _context.Favoritos
            .FirstOrDefaultAsync(f => f.ProdutoId == produtoId && f.UsuarioId == usuarioId);

    public async Task<HashSet<Guid>> IdsPorUsuario(Guid usuarioId, IEnumerable<Guid> produtoIds)
    {
        var idsProcurados = produtoIds.ToList();

        var favoritados = await _context.Favoritos
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioId && idsProcurados.Contains(f.ProdutoId))
            .Select(f => f.ProdutoId)
            .ToListAsync();

        return favoritados.ToHashSet();
    }

    public async Task Adicionar(Favorito favorito) =>
        await _context.Favoritos.AddAsync(favorito);

    public void Remover(Favorito favorito) =>
        _context.Favoritos.Remove(favorito);
}
