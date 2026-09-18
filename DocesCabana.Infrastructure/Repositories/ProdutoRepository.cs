using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
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

    public async Task<List<Produto>> BuscarPaginaDoCatalogo(FiltroCatalogoDTO filtro, int pagina, int tamanhoDaPagina)
    {
        var consulta = AplicarOrdenacao(ConstruirConsulta(filtro), filtro.Ordenacao);

        return await consulta
            .Skip((pagina - 1) * tamanhoDaPagina)
            .Take(tamanhoDaPagina)
            .ToListAsync();
    }

    public async Task<int> ContarNoCatalogo(FiltroCatalogoDTO filtro) =>
        await ConstruirConsulta(filtro).CountAsync();

    public async Task<Dictionary<Guid, int>> ContarDisponivelPorSubcategoria() =>
        await _context.Produtos
            .AsNoTracking()
            .Where(p => p.Status != ProdutoStatus.Inativo)
            .GroupBy(p => p.SubcategoriaId)
            .Select(g => new { SubcategoriaId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(g => g.SubcategoriaId, g => g.Quantidade);

    private IQueryable<Produto> ConstruirConsulta(FiltroCatalogoDTO filtro)
    {
        var consulta = _context.Produtos
            .AsNoTracking()
            .Where(p => p.Status != ProdutoStatus.Inativo);

        if (filtro.CategoriaId.HasValue)
            consulta = consulta.Where(p => p.Subcategoria!.CategoriaId == filtro.CategoriaId.Value);

        if (filtro.SubcategoriaIds.Count > 0)
            consulta = consulta.Where(p => filtro.SubcategoriaIds.Contains(p.SubcategoriaId));

        if (filtro.ApenasSemAcucar)
            consulta = consulta.Where(p => p.SemAcucar);

        if (!string.IsNullOrWhiteSpace(filtro.TermoNormalizado))
            consulta = consulta.Where(p => p.NomeNormalizado.Contains(filtro.TermoNormalizado));

        return consulta;
    }

    private IOrderedQueryable<Produto> AplicarOrdenacao(IQueryable<Produto> consulta, OrdenacaoCatalogo ordenacao) =>
        ordenacao switch
        {
            OrdenacaoCatalogo.MenorPreco => consulta.OrderBy(p => p.Preco).ThenBy(p => p.Nome),
            OrdenacaoCatalogo.MaiorPreco => consulta.OrderByDescending(p => p.Preco).ThenBy(p => p.Nome),

            OrdenacaoCatalogo.MelhorAvaliados => consulta
                .OrderByDescending(p => _context.Avaliacoes
                    .Where(a => a.ProdutoId == p.ProdutoId)
                    .Average(a => (double?)a.Nota) ?? -1)
                .ThenBy(p => p.Nome),

            OrdenacaoCatalogo.MaisVendidos => consulta
                .OrderByDescending(p => _context.ItensPedido
                    .Where(i => i.ProdutoId == p.ProdutoId && i.Pedido!.Status != PedidoStatus.Cancelado)
                    .Sum(i => (int?)i.Quantidade) ?? 0)
                .ThenBy(p => p.Nome),

            // NomeAZ é o padrão (RF-17).
            _ => consulta.OrderBy(p => p.Nome),
        };
}
