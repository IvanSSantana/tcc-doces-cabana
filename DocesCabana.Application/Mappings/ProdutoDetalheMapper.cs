using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class ProdutoDetalheMapper
{
    private const int TamanhoDoResumo = 160;

    /// <summary>
    /// RN-02: os primeiros 160 caracteres da descrição, cortados no fim de
    /// uma palavra e encerrados com reticências. Descrição com 160
    /// caracteres ou menos sai inteira, sem reticências. Nula ou vazia
    /// gera resumo nulo — RF-08 e CA-03.
    /// </summary>
    public static string? GerarResumo(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return null;

        if (descricao.Length <= TamanhoDoResumo)
            return descricao;

        var cortado = descricao[..TamanhoDoResumo];
        var ultimoEspaco = cortado.LastIndexOf(' ');
        if (ultimoEspaco > 0)
            cortado = cortado[..ultimoEspaco];

        return cortado.TrimEnd() + "…";
    }

    public static ProdutoDetalheDTO ToDTO(
        Produto produto, ResumoAvaliacoesDTO resumoAvaliacoes, PaginaAvaliacoesDTO paginaAvaliacoes) =>
        new()
        {
            ProdutoId = produto.ProdutoId,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Status = produto.Status,
            ImagemUrl = produto.ImagemUrl,
            Descricao = produto.Descricao,
            Resumo = GerarResumo(produto.Descricao),
            SubcategoriaNome = produto.Subcategoria?.Nome ?? string.Empty,
            ResumoAvaliacoes = resumoAvaliacoes,
            PaginaAvaliacoes = paginaAvaliacoes
        };
}
