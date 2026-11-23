using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Application.Mappings;

public static class AgendaDeRetiradaMapper
{
    /// <summary>Ordem de exibição: segunda a domingo.</summary>
    private static readonly DayOfWeek[] OrdemDaSemana =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    public static AgendaDeRetiradaDTO ToDTO(AgendaDeRetirada agenda) => new()
    {
        DuracaoDaFaixaEmMinutos = agenda.DuracaoDaFaixaEmMinutos,
        PreparoEmMinutos = agenda.PreparoEmMinutos,
        Horarios = OrdemDaSemana
            .Select(dia => agenda.Horarios.Single(h => h.DiaDaSemana == dia))
            .Select(h => new HorarioDeRetiradaDTO
            {
                DiaDaSemana = h.DiaDaSemana,
                Aberto = h.Aberto,
                Abertura = h.Abertura,
                Fechamento = h.Fechamento
            })
            .ToList(),
        DiasBloqueados = agenda.DiasBloqueados
            .OrderBy(d => d.Data)
            .Select(d => new DiaSemRetiradaDTO { Data = d.Data, Motivo = d.Motivo })
            .ToList()
    };

    public static FaixaDeRetiradaDTO ToDTO(FaixaDeRetirada faixa) => new(faixa.Inicio, faixa.Fim);
}
