using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class ProdutoMapper
{
    public static ProdutoDTO ToDTO(Produto produto) =>
        ToDTO(produto, estaFavorito: false);

    public static ProdutoDTO ToDTO(Produto produto, bool estaFavorito, PromocoesVigentes? promocoes = null)
    {
        var (precoPromocional, selos) = (promocoes ?? PromocoesVigentes.Nenhuma)
            .ParaVitrine(produto.ProdutoId, produto.SubcategoriaId, produto.Preco);

        return new()
        {
            ProdutoId = produto.ProdutoId,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Status = produto.Status,
            ImagemUrl = produto.ImagemUrl,
            Descricao = produto.Descricao,
            SubcategoriaId = produto.SubcategoriaId,
            SemAcucar = produto.SemAcucar,
            EstaFavorito = estaFavorito,
            Peso = produto.Peso,
            Altura = produto.Altura,
            Largura = produto.Largura,
            Comprimento = produto.Comprimento,
            PrecoPromocional = precoPromocional,
            SelosDePromocao = selos
        };
    }

    public static List<ProdutoDTO> ToDTO(IEnumerable<Produto> produtos) =>
        produtos.Select(ToDTO).ToList();

    public static List<ProdutoDTO> ToDTO(
        IEnumerable<Produto> produtos, ISet<Guid> favoritados, PromocoesVigentes? promocoes = null) =>
        produtos.Select(p => ToDTO(p, favoritados.Contains(p.ProdutoId), promocoes)).ToList();

    public static Produto ToEntity(ProdutoDTO dto) =>
        new(dto.SubcategoriaId, dto.Nome, dto.Preco, dto.ImagemUrl,
            dto.Peso, dto.Altura, dto.Largura, dto.Comprimento,
            dto.Status, dto.ProdutoId, dto.Descricao, dto.SemAcucar);

    public static List<Produto> ToEntity(IEnumerable<ProdutoDTO> dtos) =>
        dtos.Select(ToEntity).ToList();
}