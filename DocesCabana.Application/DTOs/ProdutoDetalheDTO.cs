using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

/// <summary>
/// DTO composto da página do produto — junta produto, caminho de navegação
/// e avaliações num objeto só, para a view não fazer conta nem consulta
/// (plano §1 da spec 008).
/// </summary>
public class ProdutoDetalheDTO
{
    public Guid ProdutoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public decimal Preco { get; init; }

    public ProdutoStatus Status { get; init; }

    public string ImagemUrl { get; init; } = string.Empty;

    public string? Descricao { get; init; }

    // Nulo quando não há descrição — RF-08: a view omite resumo, atalho e
    // seção inteira nesse caso, sem título órfão.
    public string? Resumo { get; init; }

    public string SubcategoriaNome { get; init; } = string.Empty;

    public ResumoAvaliacoesDTO ResumoAvaliacoes { get; init; } = new();

    public PaginaAvaliacoesDTO PaginaAvaliacoes { get; init; } = new();
}
