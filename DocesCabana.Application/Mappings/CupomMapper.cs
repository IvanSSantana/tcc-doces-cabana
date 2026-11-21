using System.Globalization;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class CupomMapper
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Rotulo(Cupom cupom) => cupom.Tipo switch
    {
        TipoDeDesconto.Percentual => $"{cupom.Valor.ToString("0.##", PtBr)}%",
        TipoDeDesconto.ValorFixo => $"R$ {cupom.Valor.ToString("N2", PtBr)}",
        TipoDeDesconto.CompreXLeveY => $"Leve {cupom.QuantidadeLeve}, pague {cupom.QuantidadePague}",
        _ => "Frete grátis",
    };

    public static string? NomeDaRestricao(Cupom cupom) =>
        cupom.CategoriaId is not null ? cupom.Categoria?.Nome
        : cupom.SubcategoriaId is not null ? cupom.Subcategoria?.Nome
        : null;

    public static ResumoDeCupomDTO ToResumoDTO(Cupom cupom, DateOnly hoje, int usos) => new(
        cupom.CupomId,
        cupom.Codigo,
        Rotulo(cupom),
        cupom.DataInicio,
        cupom.DataFim,
        DescreverRestricoes(cupom),
        usos,
        Situacao(cupom, hoje),
        cupom.Ativo);

    public static CupomDTO ToDTO(Cupom cupom) => new()
    {
        CupomId = cupom.CupomId,
        Codigo = cupom.Codigo,
        Descricao = cupom.Descricao,
        Tipo = cupom.Tipo,
        Valor = cupom.Tipo is TipoDeDesconto.Percentual or TipoDeDesconto.ValorFixo ? cupom.Valor : null,
        QuantidadeLeve = cupom.QuantidadeLeve,
        QuantidadePague = cupom.QuantidadePague,
        DataInicio = cupom.DataInicio,
        DataFim = cupom.DataFim,
        ValorMinimo = cupom.ValorMinimo,
        UmUsoPorCliente = cupom.UmUsoPorCliente,
        CategoriaId = cupom.CategoriaId,
        SubcategoriaId = cupom.SubcategoriaId,
    };

    private static SituacaoDaPromocao Situacao(Cupom cupom, DateOnly hoje)
    {
        if (!cupom.Ativo)
            return SituacaoDaPromocao.Desativada;

        if (hoje < cupom.DataInicio)
            return SituacaoDaPromocao.Agendada;

        if (hoje > cupom.DataFim)
            return SituacaoDaPromocao.Encerrada;

        return SituacaoDaPromocao.Vigente;
    }

    private static string DescreverRestricoes(Cupom cupom)
    {
        var partes = new List<string>();

        if (cupom.ValorMinimo is not null)
            partes.Add($"A partir de R$ {cupom.ValorMinimo.Value.ToString("N2", PtBr)}");

        if (cupom.CategoriaId is not null)
            partes.Add($"Categoria: {cupom.Categoria?.Nome ?? "—"}");
        else if (cupom.SubcategoriaId is not null)
            partes.Add($"Subcategoria: {cupom.Subcategoria?.Nome ?? "—"}");

        if (cupom.UmUsoPorCliente)
            partes.Add("1 uso por cliente");

        return partes.Count == 0 ? "Sem restrições" : string.Join(" · ", partes);
    }
}
