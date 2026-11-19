using DocesCabana.Domain.Helpers;
using Xunit;

namespace DocesCabana.Tests.Units.Helpers;

public class DescontoHelperTests
{
    [Fact]
    public void Dado_Leve3Pague2Com7Unidades_Quando_CalcularLeveXPagueY_Entao_DeveDescontar2Unidades()
    {
        var desconto = DescontoHelper.CalcularLeveXPagueY(12.50m, 7, 3, 2);

        Assert.Equal(25.00m, desconto);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(6, 2)]
    public void Dado_Leve3Pague2_Quando_CalcularLeveXPagueY_Entao_DeveDescontarUmaUnidadePorGrupoCompleto(
        short quantidade, int unidadesGratis)
    {
        var desconto = DescontoHelper.CalcularLeveXPagueY(10m, quantidade, 3, 2);

        Assert.Equal(unidadesGratis * 10m, desconto);
    }

    [Fact]
    public void Dado_Leve4Pague2Com8Unidades_Quando_CalcularLeveXPagueY_Entao_DeveDescontar4Unidades()
    {
        var desconto = DescontoHelper.CalcularLeveXPagueY(5m, 8, 4, 2);

        Assert.Equal(20m, desconto);
    }

    [Fact]
    public void Dado_ValorComTerceiraCasa5_Quando_Arredondar_Entao_DeveArredondarParaCima()
    {
        Assert.Equal(1.13m, DescontoHelper.Arredondar(1.125m));
    }

    [Fact]
    public void Dado_ValorComTerceiraCasa4_Quando_Arredondar_Entao_DeveArredondarParaBaixo()
    {
        Assert.Equal(1.12m, DescontoHelper.Arredondar(1.124m));
    }
}
