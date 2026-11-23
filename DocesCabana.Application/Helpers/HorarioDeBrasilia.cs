namespace DocesCabana.Application.Helpers;

public static class HorarioDeBrasilia
{
    private static readonly TimeSpan Deslocamento = TimeSpan.FromHours(-3);

    public static DateOnly Hoje(TimeProvider relogio) =>
        DateOnly.FromDateTime(Agora(relogio));

    /// <summary>Data e hora locais de Brasília, sem fuso (Kind = Unspecified).</summary>
    public static DateTime Agora(TimeProvider relogio) =>
        relogio.GetUtcNow().ToOffset(Deslocamento).DateTime;

    /// <summary>Converte um horário de Brasília (sem fuso) para UTC (Kind = Utc).</summary>
    public static DateTime ParaUtc(DateTime horarioDeBrasilia) =>
        DateTime.SpecifyKind(horarioDeBrasilia - Deslocamento, DateTimeKind.Utc);

    /// <summary>Converte um horário UTC para o de Brasília, sem fuso (Kind = Unspecified).</summary>
    public static DateTime DeUtc(DateTime utc) =>
        DateTime.SpecifyKind(utc + Deslocamento, DateTimeKind.Unspecified);
}
