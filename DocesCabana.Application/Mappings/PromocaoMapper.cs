using System.Globalization;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class PromocaoMapper
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Rotulo(Promocao promocao) => promocao.Tipo switch
    {
        TipoDeDesconto.Percentual => $"-{promocao.Valor.ToString("0.##", PtBr)}%",
        TipoDeDesconto.ValorFixo => $"-R$ {promocao.Valor.ToString("N2", PtBr)}",
        TipoDeDesconto.CompreXLeveY => $"Leve {promocao.QuantidadeLeve}, pague {promocao.QuantidadePague}",
        _ => promocao.Nome,
    };

    public static ResumoDePromocaoDTO ToResumoDTO(Promocao promocao, DateOnly hoje) => new(
        promocao.PromocaoId,
        promocao.Nome,
        Rotulo(promocao),
        promocao.DataInicio,
        promocao.DataFim,
        DescreverAbrangencia(promocao),
        Situacao(promocao, hoje),
        promocao.Ativa);

    public static PromocaoDTO ToDTO(Promocao promocao) => new()
    {
        PromocaoId = promocao.PromocaoId,
        Nome = promocao.Nome,
        Descricao = promocao.Descricao,
        Tipo = promocao.Tipo,
        Valor = promocao.Tipo == TipoDeDesconto.CompreXLeveY ? null : promocao.Valor,
        QuantidadeLeve = promocao.QuantidadeLeve,
        QuantidadePague = promocao.QuantidadePague,
        DataInicio = promocao.DataInicio,
        DataFim = promocao.DataFim,
        Abrangencia = promocao.Abrangencia,
        CategoriaId = promocao.CategoriaId,
        SubcategoriaId = promocao.SubcategoriaId,
        ProdutoIds = promocao.Produtos.Select(p => p.ProdutoId).ToList(),
    };

    private static SituacaoDaPromocao Situacao(Promocao promocao, DateOnly hoje)
    {
        if (!promocao.Ativa)
            return SituacaoDaPromocao.Desativada;

        if (hoje < promocao.DataInicio)
            return SituacaoDaPromocao.Agendada;

        if (hoje > promocao.DataFim)
            return SituacaoDaPromocao.Encerrada;

        return SituacaoDaPromocao.Vigente;
    }

    private static string DescreverAbrangencia(Promocao promocao) => promocao.Abrangencia switch
    {
        AbrangenciaDaPromocao.Categoria => $"Categoria: {promocao.Categoria?.Nome ?? "—"}",
        AbrangenciaDaPromocao.Subcategoria => $"Subcategoria: {promocao.Subcategoria?.Nome ?? "—"}",
        _ => promocao.Produtos.Count == 1 ? "1 produto" : $"{promocao.Produtos.Count} produtos",
    };
}
