using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class ProdutoMapper
{
    public static ProdutoDTO ToDTO(Produto produto) =>
        ToDTO(produto, estaFavorito: false);

    // EstaFavorito existe no DTO desde a spec 012 e nunca tinha sido
    // preenchido — a spec 015 é a primeira a ter de onde vir esse dado.
    public static ProdutoDTO ToDTO(Produto produto, bool estaFavorito) =>
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
            SemAcucar = produto.SemAcucar,
            EstaFavorito = estaFavorito
        };

    public static List<ProdutoDTO> ToDTO(IEnumerable<Produto> produtos) =>
        produtos.Select(ToDTO).ToList();

    // Uma consulta de favoritos por página, não uma por cartão (spec 015,
    // RF-02, plano §5) — quem chama já trouxe o conjunto pronto.
    public static List<ProdutoDTO> ToDTO(IEnumerable<Produto> produtos, ISet<Guid> favoritados) =>
        produtos.Select(p => ToDTO(p, favoritados.Contains(p.ProdutoId))).ToList();

    public static Produto ToEntity(ProdutoDTO dto) =>
        new(dto.SubcategoriaId, dto.Nome, dto.Preco, dto.ImagemUrl, dto.Status, dto.ProdutoId, dto.Descricao, dto.SemAcucar);

    public static List<Produto> ToEntity(IEnumerable<ProdutoDTO> dtos) =>
        dtos.Select(ToEntity).ToList();
}