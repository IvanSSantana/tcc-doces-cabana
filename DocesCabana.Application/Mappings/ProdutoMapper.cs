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
        produtos.Select(ToDTO).ToList();

    public static Produto ToEntity(ProdutoDTO dto) =>
        new(dto.SubcategoriaId, dto.Nome, dto.Preco, dto.ImagemUrl, dto.Status, dto.Id);

    public static List<Produto> ToEntity(IEnumerable<ProdutoDTO> dtos) =>
        dtos.Select(ToEntity).ToList();
}