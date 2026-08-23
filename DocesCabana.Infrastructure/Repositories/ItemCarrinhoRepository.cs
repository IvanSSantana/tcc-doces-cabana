using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class ItemCarrinhoRepository : IItemCarrinhoRepository
{
    private readonly DocesCabanaDbContext _context;

    public ItemCarrinhoRepository(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemCarrinho>> BuscarPorUsuario(Guid usuarioId) =>
        await _context.ItensCarrinho
            .Include(i => i.Produto)
            .Where(i => i.UsuarioId == usuarioId)
            .ToListAsync();

    // Sem AsNoTracking: o item volta rastreado porque AlterarQuantidade e
    // Acrescentar mutam o estado em memória, e SalvarAlteracoes precisa que
    // o ChangeTracker perceba a mudança.
    public async Task<ItemCarrinho?> Buscar(Guid usuarioId, Guid produtoId) =>
        await _context.ItensCarrinho
            .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId && i.ProdutoId == produtoId);

    public async Task<int> ContarItens(Guid usuarioId) =>
        await _context.ItensCarrinho
            .AsNoTracking()
            .Where(i => i.UsuarioId == usuarioId)
            .SumAsync(i => (int)i.Quantidade);

    public async Task Adicionar(ItemCarrinho item) =>
        await _context.ItensCarrinho.AddAsync(item);

    public void Remover(ItemCarrinho item) =>
        _context.ItensCarrinho.Remove(item);
}
