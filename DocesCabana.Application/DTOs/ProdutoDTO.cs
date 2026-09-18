using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class ProdutoDTO
{
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

    public decimal Peso { get; init; }

    public decimal Altura { get; init; }

    public decimal Largura { get; init; }

    public decimal Comprimento { get; init; }

    public ProdutoDTO ComImagem(string imagemUrl) => new()
    {
        ProdutoId = ProdutoId,
        Nome = Nome,
        Preco = Preco,
        Status = Status,
        ImagemUrl = imagemUrl,
        Descricao = Descricao,
        SubcategoriaId = SubcategoriaId,
        PromocaoId = PromocaoId,
        EstaFavorito = EstaFavorito,
        SemAcucar = SemAcucar,
        Peso = Peso,
        Altura = Altura,
        Largura = Largura,
        Comprimento = Comprimento
    };
}
