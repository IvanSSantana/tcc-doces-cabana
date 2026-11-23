using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Services;

public class PainelDeVendasService : IPainelDeVendasService
{
    private const int QuantidadeDeMaisVendidos = 10;

    private static readonly string[] DiasDaSemana = ["seg", "ter", "qua", "qui", "sex", "sáb", "dom"];

    private static readonly string[] Meses =
        ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    private readonly IPedidoRepository _pedidoRepository;
    private readonly TimeProvider _relogio;

    public PainelDeVendasService(IPedidoRepository pedidoRepository, TimeProvider relogio)
    {
        _pedidoRepository = pedidoRepository;
        _relogio = relogio;
    }

    public async Task<PainelDeVendasDTO> Montar(PeriodoDoPainel periodo, DateOnly? referencia)
    {
        if (!Enum.IsDefined(periodo))
            periodo = PeriodoDoPainel.Mes;

        var hoje = HorarioDeBrasilia.Hoje(_relogio);
        var atual = IntervaloDoPainel.Calcular(periodo, referencia ?? hoje);
        var anterior = atual.Anterior();
        var seguinte = atual.Seguinte();

        var vendasAtuais = await BuscarVendas(atual);
        var vendasAnteriores = await BuscarVendas(anterior);

        var receitaAtual = vendasAtuais.Sum(v => v.Receita);
        var receitaAnterior = vendasAnteriores.Sum(v => v.Receita);

        return new PainelDeVendasDTO(
            periodo,
            atual.Referencia,
            atual.Rotulo,
            anterior.Referencia,
            seguinte.Referencia > hoje ? null : seguinte.Referencia,
            Indicador(receitaAtual, receitaAnterior),
            Indicador(vendasAtuais.Count, vendasAnteriores.Count),
            Indicador(TicketMedio(receitaAtual, vendasAtuais.Count), TicketMedio(receitaAnterior, vendasAnteriores.Count)),
            MontarSerie(atual, vendasAtuais),
            MontarMaisVendidos(vendasAtuais),
            periodo == PeriodoDoPainel.Dia ? [] : MontarPorDiaDaSemana(vendasAtuais),
            periodo == PeriodoDoPainel.Dia ? [] : MontarPorHora(vendasAtuais));
    }

    /// <summary>Pedido do intervalo com a data já em Brasília e a receita sem frete.</summary>
    private sealed record Venda(DateTime Data, decimal Receita, Pedido Pedido);

    private async Task<List<Venda>> BuscarVendas(IntervaloDoPainel intervalo)
    {
        var pedidos = await _pedidoRepository.ListarReceitaEntre(
            HorarioDeBrasilia.ParaUtc(intervalo.Inicio), HorarioDeBrasilia.ParaUtc(intervalo.Fim));

        return pedidos
            .Select(p => new Venda(HorarioDeBrasilia.DeUtc(p.Data), p.ValorDosProdutos(), p))
            .ToList();
    }

    private static decimal TicketMedio(decimal receita, int pedidos) =>
        pedidos == 0 ? 0m : Math.Round(receita / pedidos, 2);

    private static IndicadorDoPainelDTO Indicador(decimal atual, decimal anterior) =>
        new(atual, anterior, anterior == 0m ? null : Math.Round((atual - anterior) / anterior * 100m, 1));

    private static IReadOnlyList<PontoDoGraficoDTO> MontarSerie(IntervaloDoPainel intervalo, List<Venda> vendas) =>
        intervalo.Periodo switch
        {
            PeriodoDoPainel.Dia => Agrupar(vendas, 24, v => v.Data.Hour, h => $"{h:D2}h"),
            PeriodoDoPainel.Semana => Agrupar(
                vendas, 7, v => (v.Data.Date - intervalo.Inicio).Days,
                d => $"{DiasDaSemana[d]} {intervalo.Inicio.AddDays(d):dd/MM}"),
            PeriodoDoPainel.Mes => Agrupar(
                vendas, (intervalo.Fim - intervalo.Inicio).Days, v => v.Data.Day - 1, d => $"{d + 1:D2}"),
            _ => Agrupar(vendas, 12, v => v.Data.Month - 1, m => Meses[m])
        };

    private static IReadOnlyList<PontoDoGraficoDTO> MontarPorDiaDaSemana(List<Venda> vendas) =>
        Agrupar(vendas, 7, v => ((int)v.Data.DayOfWeek + 6) % 7, d => DiasDaSemana[d]);

    private static IReadOnlyList<PontoDoGraficoDTO> MontarPorHora(List<Venda> vendas) =>
        Agrupar(vendas, 24, v => v.Data.Hour, h => $"{h:D2}h");

    /// <summary>Uma faixa por posição, inclusive as sem venda (zeradas).</summary>
    private static IReadOnlyList<PontoDoGraficoDTO> Agrupar(
        List<Venda> vendas, int faixas, Func<Venda, int> faixaDaVenda, Func<int, string> rotulo)
    {
        var porFaixa = vendas.ToLookup(faixaDaVenda);

        return Enumerable.Range(0, faixas)
            .Select(f => new PontoDoGraficoDTO(rotulo(f), porFaixa[f].Sum(v => v.Receita), porFaixa[f].Count()))
            .ToList();
    }

    private static IReadOnlyList<ProdutoMaisVendidoDTO> MontarMaisVendidos(List<Venda> vendas) =>
        vendas
            .SelectMany(v => v.Pedido.Itens)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new ProdutoMaisVendidoDTO(
                g.Key,
                g.First().Produto?.Nome ?? "Produto removido",
                g.Sum(i => (int)i.Quantidade),
                g.Sum(i => i.CalcularValorDaLinha())))
            .OrderByDescending(p => p.Quantidade)
            .ThenByDescending(p => p.Receita)
            .ThenBy(p => p.Nome)
            .Take(QuantidadeDeMaisVendidos)
            .ToList();
}
