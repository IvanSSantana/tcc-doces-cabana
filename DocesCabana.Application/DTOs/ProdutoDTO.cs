using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class ProdutoDTO
{
    // O uso do init se dá pois o DTO é imutável e não deve ser alterado após a criação. Afinal é vindo do banco de dados.
    public Guid ProdutoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public decimal Preco { get; init; }

    public ProdutoStatus Status { get; init; }

    public string ImagemUrl { get; init; } = string.Empty;

    public string? Descricao { get; init; }

    public Guid SubcategoriaId { get; init; }

    public Guid? PromocaoId { get; init; }

    public bool EstaFavorito { get; init; }

    public bool SemAcucar { get; init; }

    // Peso e dimensões (spec 020, RF-01) — kg e cm, mesma unidade do domínio.
    public decimal Peso { get; init; }

    public decimal Altura { get; init; }

    public decimal Largura { get; init; }

    public decimal Comprimento { get; init; }
}
