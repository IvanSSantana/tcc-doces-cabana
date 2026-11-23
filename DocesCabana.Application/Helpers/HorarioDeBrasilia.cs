namespace DocesCabana.Application.Helpers;

public static class HorarioDeBrasilia
{
    private static readonly TimeSpan Deslocamento = TimeSpan.FromHours(-3);

    public static DateOnly Hoje(TimeProvider relogio) =>
        DateOnly.FromDateTime(Agora(relogio));

    /// <summary>Data e hora locais de Brasília, sem fuso (Kind = Unspecified).</summary>
    public static DateTime Agora(TimeProvider relogio) =>
        relogio.GetUtcNow().ToOffset(Deslocamento).DateTime;
}
