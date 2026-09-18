using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class ProdutoDetalheDTO
{
    public Guid ProdutoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public decimal Preco { get; init; }

    public ProdutoStatus Status { get; init; }

    public string ImagemUrl { get; init; } = string.Empty;

    public string? Descricao { get; init; }

    public string? Resumo { get; init; }

    public string SubcategoriaNome { get; init; } = string.Empty;

    public ResumoAvaliacoesDTO ResumoAvaliacoes { get; init; } = new();

    public PaginaAvaliacoesDTO PaginaAvaliacoes { get; init; } = new();
}
