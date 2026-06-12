using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class ProdutoMapper
{
    public static ProdutoDTO ToDTO(Produto produto) =>
        new()
        {
            Id = produto.ProdutoId,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Status = produto.Status,
            ImagemUrl = produto.ImagemUrl,
            SubcategoriaId = produto.SubcategoriaId,
            PromocaoId = produto.PromocaoId
        };

    public static List<ProdutoDTO> ToDTO(IEnumerable<Produto> produtos) =>
        produtos.Select(ToDto).ToList();
}
