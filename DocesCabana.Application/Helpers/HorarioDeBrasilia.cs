namespace DocesCabana.Application.Helpers;

public static class HorarioDeBrasilia
{
    private static readonly TimeSpan Deslocamento = TimeSpan.FromHours(-3);

    public static DateOnly Hoje(TimeProvider relogio) =>
        DateOnly.FromDateTime(relogio.GetUtcNow().ToOffset(Deslocamento).DateTime);
}
