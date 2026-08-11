using DocesCabana.Domain.Helpers;
using Xunit;

namespace DocesCabana.Tests.Units.Helpers;

public class TelefoneHelperTests
{
    [Theory]
    [InlineData("11987654321")]
    [InlineData("(11) 98765-4321")]
    [InlineData("21987654321")]
    public void Dado_CelularComDddENonoDigitoValidos_Quando_CelularValido_Entao_DeveRetornarTrue(string celular)
    {
        Assert.True(TelefoneHelper.CelularValido(celular));
    }

    [Theory]
    [InlineData("1187654321")]
    [InlineData("00987654321")]
    [InlineData("119876543210")]
    [InlineData("")]
    public void Dado_CelularComDddOuNonoDigitoInvalidos_Quando_CelularValido_Entao_DeveRetornarFalse(string celular)
    {
        Assert.False(TelefoneHelper.CelularValido(celular));
    }

    [Fact]
    public void Dado_CelularFormatado_Quando_ApenasDigitos_Entao_DeveRemoverFormatacao()
    {
        var resultado = TelefoneHelper.ApenasDigitos("(11) 98765-4321");

        Assert.Equal("11987654321", resultado);
    }
}
