using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class AvaliacaoRepository : Repository<Avaliacao>, IAvaliacaoRepository
{
    public AvaliacaoRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Avaliacao>> BuscarPorProduto(Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade)
    {
        var consulta = _context.Avaliacoes
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Include(a => a.Votos)
            .Where(a => a.ProdutoId == produtoId);

        // RN-05: Relevantes ordena por mais úteis, empate pela mais recente.
        // "Votos.Count" é seguro como contagem de pessoas distintas porque a
        // chave composta de VotoUtil impede duplicidade de (Avaliacao, Usuario).
        consulta = ordenacao switch
        {
            OrdenacaoAvaliacao.MaisRecentes => consulta.OrderByDescending(a => a.DataCriacao),
            OrdenacaoAvaliacao.MaiorNota => consulta.OrderByDescending(a => a.Nota).ThenByDescending(a => a.DataCriacao),
            OrdenacaoAvaliacao.MenorNota => consulta.OrderBy(a => a.Nota).ThenByDescending(a => a.DataCriacao),
            _ => consulta.OrderByDescending(a => a.Votos.Count).ThenByDescending(a => a.DataCriacao),
        };

        return await consulta.Take(quantidade).ToListAsync();
    }

    public async Task<int> ContarPorProduto(Guid produtoId) =>
        await _context.Avaliacoes.AsNoTracking().CountAsync(a => a.ProdutoId == produtoId);

    public async Task<IReadOnlyDictionary<byte, int>> ContarPorNota(Guid produtoId)
    {
        var contagens = await _context.Avaliacoes
            .AsNoTracking()
            .Where(a => a.ProdutoId == produtoId)
            .GroupBy(a => a.Nota)
            .Select(g => new { Nota = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return contagens.ToDictionary(c => c.Nota, c => c.Quantidade);
    }

    public async Task<Avaliacao?> BuscarComVotos(Guid avaliacaoId) =>
        // Sem AsNoTracking: o voto precisa ficar rastreado para o
        // ChangeTracker perceber o item acrescentado/removido de Votos
        // quando AlternarVotoUtil mutar a coleção em memória.
        await _context.Avaliacoes
            .Include(a => a.Votos)
            .FirstOrDefaultAsync(a => a.AvaliacaoId == avaliacaoId);
}
