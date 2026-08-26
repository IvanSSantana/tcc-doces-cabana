using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<Pedido?> BuscarPorIdComItens(Guid pedidoId) =>
        await _context.Pedidos
            .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
            .Include(p => p.EnderecoEntrega)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

    public async Task<List<Pedido>> ListarPorUsuario(Guid usuarioId) =>
        await _context.Pedidos
            .Where(p => p.UsuarioId == usuarioId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Pagamento?> BuscarPagamentoPorPedido(Guid pedidoId) =>
        await _context.Pagamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

    public async Task AdicionarComPagamento(Pedido pedido, Pagamento pagamento)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.Pagamentos.AddAsync(pagamento);
    }
}
