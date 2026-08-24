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

    // RF-01/RN-01: o penúltimo dígito (primeiro verificador) nunca era
    // conferido — só o último. Estes três têm o segundo dígito "certo" por
    // coincidência (foi recalculado a partir do primeiro, errado, então
    // "EndsWith" batia) e o primeiro digitado errado, e passavam antes desta
    // correção (spec 019).
    [Theory]
    [InlineData("52998224795")]
    [InlineData("52998224705")]
    [InlineData("52998224715")]
    public void Dado_CpfComPrimeiroDigitoVerificadorErrado_Quando_CpfValido_Entao_DeveRetornarFalse(string cpf)
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

    // CA-05: os nove CPFs que o seed grava na subida da aplicação — os oito
    // clientes de DbInitializer.SemearContasDeDemonstracao e o administrador
    // de SemearAdministrador (DocesCabana.MVC/Helpers/DbInitializer.cs:362) —
    // conferidos dígito a dígito ao especificar a spec 019. Se algum falhar
    // aqui, o seed tem CPF inválido e a correção do CpfHelper derruba a
    // aplicação na subida: é o teste de guarda que a Fase 2 pede rodar antes
    // de tudo o mais.
    [Theory]
    [InlineData("87654321937")]
    [InlineData("11144477735")]
    [InlineData("39053344705")]
    [InlineData("45678912364")]
    [InlineData("01234567890")]
    [InlineData("12345678909")]
    [InlineData("98765432100")]
    [InlineData("11223344517")]
    [InlineData("52998224725")]
    public void Dado_CpfSemeadoNaSubidaDaAplicacao_Quando_CpfValido_Entao_DeveRetornarTrue(string cpf)
    {
        Assert.True(CpfHelper.CpfValido(cpf));
    }
}
