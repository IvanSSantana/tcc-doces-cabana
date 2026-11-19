using DocesCabana.Application.Helpers;
using Moq;

namespace DocesCabana.Tests.Units.Helpers;

public class HorarioDeBrasiliaTests
{
    private static TimeProvider RelogioEm(DateTimeOffset utc)
    {
        var relogio = new Mock<TimeProvider>();
        relogio.Setup(r => r.GetUtcNow()).Returns(utc);
        return relogio.Object;
    }

    [Fact]
    public void Dado_UtcAs2h59_Quando_Hoje_Entao_DeveSerDiaAnterior()
    {
        var hoje = HorarioDeBrasilia.Hoje(RelogioEm(new DateTimeOffset(2026, 11, 20, 2, 59, 59, TimeSpan.Zero)));

        Assert.Equal(new DateOnly(2026, 11, 19), hoje);
    }

    [Fact]
    public void Dado_UtcAs3h00_Quando_Hoje_Entao_DeveSerMesmoDia()
    {
        var hoje = HorarioDeBrasilia.Hoje(RelogioEm(new DateTimeOffset(2026, 11, 20, 3, 0, 0, TimeSpan.Zero)));

        Assert.Equal(new DateOnly(2026, 11, 20), hoje);
    }
}
