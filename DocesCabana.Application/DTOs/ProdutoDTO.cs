using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class ProdutoDTO
{
    // O uso do init se dá pois o DTO é imutável e não deve ser alterado após a criação. Afinal é vindo do banco de dados.
    public Guid Id { get; init; }

    public string Nome { get; init; } = string.Empty;

    public decimal Preco { get; init; }

    public ProdutoStatus Status { get; init; }

    public string ImagemUrl { get; init; } = string.Empty;

    public Guid SubcategoriaId { get; init; }

    public Guid? PromocaoId { get; init; }

    public bool IsFavorite { get; set; }
}
