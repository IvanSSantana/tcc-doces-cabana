using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

/// <summary>Tudo o que a tela de vendas mostra para um período. Valores de receita sem frete.</summary>
public record PainelDeVendasDTO(
    PeriodoDoPainel Periodo,
    DateOnly Referencia,
    string RotuloDoPeriodo,
    DateOnly ReferenciaAnterior,
    DateOnly? ReferenciaSeguinte,
    IndicadorDoPainelDTO Receita,
    IndicadorDoPainelDTO Pedidos,
    IndicadorDoPainelDTO TicketMedio,
    IReadOnlyList<PontoDoGraficoDTO> Serie,
    IReadOnlyList<ProdutoMaisVendidoDTO> MaisVendidos,
    IReadOnlyList<PontoDoGraficoDTO> PorDiaDaSemana,
    IReadOnlyList<PontoDoGraficoDTO> PorHora);
