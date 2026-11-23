using DocesCabana.Application.Enums;
using DocesCabana.Application.Helpers;

namespace DocesCabana.Tests.Units.Helpers;

public class IntervaloDoPainelTests
{
    [Fact]
    public void Dado_PeriodoDia_Quando_Calcular_Entao_DeveIrDaMeiaNoiteAteAMeiaNoiteSeguinte()
    {
        var intervalo = IntervaloDoPainel.Calcular(PeriodoDoPainel.Dia, new DateOnly(2026, 11, 23));

        Assert.Equal(new DateTime(2026, 11, 23), intervalo.Inicio);
        Assert.Equal(new DateTime(2026, 11, 24), intervalo.Fim);
        Assert.Equal("23/11/2026", intervalo.Rotulo);
    }

    [Theory]
    [InlineData(2026, 11, 22)] // domingo
    [InlineData(2026, 11, 16)] // segunda
    [InlineData(2026, 11, 19)] // quinta
    public void Dado_ReferenciaDentroDaSemana_Quando_CalcularSemana_Entao_InicioDeveSerASegundaFeira(int ano, int mes, int dia)
    {
        var intervalo = IntervaloDoPainel.Calcular(PeriodoDoPainel.Semana, new DateOnly(ano, mes, dia));

        Assert.Equal(new DateTime(2026, 11, 16), intervalo.Inicio);
        Assert.Equal(new DateTime(2026, 11, 23), intervalo.Fim);
        Assert.Equal("16/11 a 22/11/2026", intervalo.Rotulo);
    }

    [Fact]
    public void Dado_SemanaQueCruzaOAno_Quando_Calcular_Entao_RotuloDeveTrazerOsDoisAnos()
    {
        var intervalo = IntervaloDoPainel.Calcular(PeriodoDoPainel.Semana, new DateOnly(2027, 1, 1));

        Assert.Equal(new DateTime(2026, 12, 28), intervalo.Inicio);
        Assert.Equal("28/12/2026 a 03/01/2027", intervalo.Rotulo);
    }

    [Fact]
    public void Dado_PeriodoMes_Quando_Calcular_Entao_DeveCobrirOMesInteiroComRotuloPorExtenso()
    {
        var intervalo = IntervaloDoPainel.Calcular(PeriodoDoPainel.Mes, new DateOnly(2026, 2, 17));

        Assert.Equal(new DateTime(2026, 2, 1), intervalo.Inicio);
        Assert.Equal(new DateTime(2026, 3, 1), intervalo.Fim);
        Assert.Equal("fevereiro de 2026", intervalo.Rotulo);
    }

    [Fact]
    public void Dado_PeriodoAno_Quando_Calcular_Entao_DeveCobrirOAnoInteiro()
    {
        var intervalo = IntervaloDoPainel.Calcular(PeriodoDoPainel.Ano, new DateOnly(2026, 7, 4));

        Assert.Equal(new DateTime(2026, 1, 1), intervalo.Inicio);
        Assert.Equal(new DateTime(2027, 1, 1), intervalo.Fim);
        Assert.Equal("2026", intervalo.Rotulo);
    }

    [Fact]
    public void Dado_Janeiro_Quando_Anterior_Entao_DeveSerDezembroDoAnoAnterior()
    {
        var anterior = IntervaloDoPainel.Calcular(PeriodoDoPainel.Mes, new DateOnly(2026, 1, 15)).Anterior();

        Assert.Equal(new DateTime(2025, 12, 1), anterior.Inicio);
        Assert.Equal(new DateTime(2026, 1, 1), anterior.Fim);
    }

    [Fact]
    public void Dado_Dezembro_Quando_Seguinte_Entao_DeveSerJaneiroDoAnoSeguinte()
    {
        var seguinte = IntervaloDoPainel.Calcular(PeriodoDoPainel.Mes, new DateOnly(2026, 12, 15)).Seguinte();

        Assert.Equal(new DateTime(2027, 1, 1), seguinte.Inicio);
        Assert.Equal(new DateTime(2027, 2, 1), seguinte.Fim);
    }

    [Fact]
    public void Dado_Semana_Quando_AnteriorESeguinte_Entao_DevemAndarSeteDias()
    {
        var semana = IntervaloDoPainel.Calcular(PeriodoDoPainel.Semana, new DateOnly(2026, 11, 19));

        Assert.Equal(new DateTime(2026, 11, 9), semana.Anterior().Inicio);
        Assert.Equal(new DateTime(2026, 11, 23), semana.Seguinte().Inicio);
    }
}
