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

    // RN-01: produto inativo não existe do lado de fora, em nenhum caminho de
    // consulta do catálogo.
    private IQueryable<Produto> ConstruirConsulta(FiltroCatalogoDTO filtro)
    {
        var consulta = _context.Produtos
            .AsNoTracking()
            .Where(p => p.Status != ProdutoStatus.Inativo);

        if (filtro.CategoriaId.HasValue)
            consulta = consulta.Where(p => p.Subcategoria!.CategoriaId == filtro.CategoriaId.Value);

        // RN-03: subcategorias marcadas se somam (OR), não intersectam.
        if (filtro.SubcategoriaIds.Count > 0)
            consulta = consulta.Where(p => filtro.SubcategoriaIds.Contains(p.SubcategoriaId));

        // RN-04: "sem açúcar" é característica do produto, combina com
        // subcategoria em vez de competir com ela (é um AND com o filtro
        // acima, não um OR).
        if (filtro.ApenasSemAcucar)
            consulta = consulta.Where(p => p.SemAcucar);

        return consulta;
    }

    // RN-05: toda ordenação termina em Nome como desempate — sem isso,
    // Skip/Take não tem resultado determinístico entre páginas (plano §9).
    private IOrderedQueryable<Produto> AplicarOrdenacao(IQueryable<Produto> consulta, OrdenacaoCatalogo ordenacao) =>
        ordenacao switch
        {
            OrdenacaoCatalogo.MenorPreco => consulta.OrderBy(p => p.Preco).ThenBy(p => p.Nome),
            OrdenacaoCatalogo.MaiorPreco => consulta.OrderByDescending(p => p.Preco).ThenBy(p => p.Nome),

            // Produto sem nenhuma avaliação vai para o fim (média nula vira
            // -1, que nenhuma nota real alcança), não descartado da consulta.
            OrdenacaoCatalogo.MelhorAvaliados => consulta
                .OrderByDescending(p => _context.Avaliacoes
                    .Where(a => a.ProdutoId == p.ProdutoId)
                    .Average(a => (double?)a.Nota) ?? -1)
                .ThenBy(p => p.Nome),

            // NomeAZ é o padrão (RF-17); MaisVendidos nunca chega aqui — o
            // controller saneia para NomeAZ antes de montar o filtro, porque
            // RN-07 a mantém indisponível até a spec 017.
            _ => consulta.OrderBy(p => p.Nome),
        };
}
