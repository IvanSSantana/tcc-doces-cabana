using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class ProdutoMapper
{
    public static ProdutoDTO ToDTO(Produto produto) =>
        new()
        {
            ProdutoId = produto.ProdutoId,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Status = produto.Status,
            ImagemUrl = produto.ImagemUrl,
            Descricao = produto.Descricao,
            SubcategoriaId = produto.SubcategoriaId,
            PromocaoId = produto.PromocaoId,
            SemAcucar = produto.SemAcucar
        };

    public static List<ProdutoDTO> ToDTO(IEnumerable<Produto> produtos) =>
        produtos.Select(ToDTO).ToList();

    public static Produto ToEntity(ProdutoDTO dto) =>
        new(dto.SubcategoriaId, dto.Nome, dto.Preco, dto.ImagemUrl, dto.Status, dto.ProdutoId, dto.Descricao, dto.SemAcucar);

    public static List<Produto> ToEntity(IEnumerable<ProdutoDTO> dtos) =>
        dtos.Select(ToEntity).ToList();
}