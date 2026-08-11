using DocesCabana.Domain.Helpers;
using Xunit;

namespace DocesCabana.Tests.Units.Helpers;

public class CpfHelperTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Dado_CpfComDigitoVerificadorValido_Quando_CpfValido_Entao_DeveRetornarTrue(string cpf)
    {
        Assert.True(CpfHelper.CpfValido(cpf));
    }

    [Theory]
    [InlineData("529.982.247-26")]
    [InlineData("52998224726")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Dado_CpfComDigitoVerificadorInvalido_Quando_CpfValido_Entao_DeveRetornarFalse(string cpf)
    {
        Assert.False(CpfHelper.CpfValido(cpf));
    }

    [Fact]
    public void Dado_CpfComOnzeDigitos_Quando_FormatoValido_Entao_DeveRetornarTrue()
    {
        Assert.True(CpfHelper.FormatoValido("529.982.247-25"));
    }

    [Theory]
    [InlineData("5299822472")]
    [InlineData("")]
    [InlineData("529982247255")]
    public void Dado_QuantidadeDeDigitosDiferenteDeOnze_Quando_FormatoValido_Entao_DeveRetornarFalse(string cpf)
    {
        Assert.False(CpfHelper.FormatoValido(cpf));
    }

    [Fact]
    public void Dado_CpfPontuado_Quando_ApenasDigitos_Entao_DeveRemoverPontuacao()
    {
        var resultado = CpfHelper.ApenasDigitos("529.982.247-25");

        Assert.Equal("52998224725", resultado);
    }
}
