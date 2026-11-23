using System.Globalization;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Helpers;

/// <summary>Período do painel de vendas no horário de Brasília, com fim exclusivo.</summary>
public sealed record IntervaloDoPainel(PeriodoDoPainel Periodo, DateTime Inicio, DateTime Fim)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static IntervaloDoPainel Calcular(PeriodoDoPainel periodo, DateOnly referencia)
    {
        var dia = referencia.ToDateTime(TimeOnly.MinValue);

        return periodo switch
        {
            PeriodoDoPainel.Dia => new(periodo, dia, dia.AddDays(1)),
            PeriodoDoPainel.Semana => SemanaDe(dia),
            PeriodoDoPainel.Mes => new(periodo, new DateTime(dia.Year, dia.Month, 1), new DateTime(dia.Year, dia.Month, 1).AddMonths(1)),
            PeriodoDoPainel.Ano => new(periodo, new DateTime(dia.Year, 1, 1), new DateTime(dia.Year + 1, 1, 1)),
            _ => throw new ArgumentOutOfRangeException(nameof(periodo), "Período do painel inválido.")
        };
    }

    public DateOnly Referencia => DateOnly.FromDateTime(Inicio);

    public IntervaloDoPainel Anterior() => Calcular(Periodo, DateOnly.FromDateTime(Inicio.AddDays(-1)));

    public IntervaloDoPainel Seguinte() => Calcular(Periodo, DateOnly.FromDateTime(Fim));

    public string Rotulo => Periodo switch
    {
        PeriodoDoPainel.Dia => Inicio.ToString("dd/MM/yyyy", PtBr),
        PeriodoDoPainel.Semana => RotuloDaSemana(),
        PeriodoDoPainel.Mes => Inicio.ToString("MMMM 'de' yyyy", PtBr),
        _ => Inicio.ToString("yyyy", PtBr)
    };

    private static IntervaloDoPainel SemanaDe(DateTime dia)
    {
        var diasDesdeSegunda = ((int)dia.DayOfWeek + 6) % 7;
        var segunda = dia.AddDays(-diasDesdeSegunda);
        return new(PeriodoDoPainel.Semana, segunda, segunda.AddDays(7));
    }

    private string RotuloDaSemana()
    {
        var ultimoDia = Fim.AddDays(-1);
        var formatoDoInicio = Inicio.Year == ultimoDia.Year ? "dd/MM" : "dd/MM/yyyy";
        return $"{Inicio.ToString(formatoDoInicio, PtBr)} a {ultimoDia.ToString("dd/MM/yyyy", PtBr)}";
    }
}
