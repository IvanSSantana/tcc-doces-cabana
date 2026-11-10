using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<Pedido?> Buscar(Guid pedidoId, Guid usuarioId) =>
        await _context.Pedidos
            .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
            .Include(p => p.EnderecoEntrega)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId && p.UsuarioId == usuarioId);

    public async Task<List<Pedido>> ListarPorUsuario(Guid usuarioId) =>
        await _context.Pedidos
            .Where(p => p.UsuarioId == usuarioId)
            .Include(p => p.Itens)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId) =>
        await _context.Pagamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

    public async Task<bool> ExisteEntregueComProduto(Guid usuarioId, Guid produtoId) =>
        await _context.Pedidos
            .AsNoTracking()
            .AnyAsync(p => p.UsuarioId == usuarioId
                && p.Status == PedidoStatus.Entregue
                && p.Itens.Any(i => i.ProdutoId == produtoId));

    public async Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.Pagamentos.AddAsync(pagamento);
    }
}
