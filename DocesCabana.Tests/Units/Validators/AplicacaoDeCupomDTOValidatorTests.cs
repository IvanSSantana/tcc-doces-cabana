using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class AplicacaoDeCupomDTOValidatorTests
{
    private readonly AplicacaoDeCupomDTOValidator _validator = new();

    [Fact]
    public void Dado_CodigoPreenchido_Quando_Validar_Entao_DeveSerValido()
    {
        Assert.True(_validator.Validate(new AplicacaoDeCupomDTO { Codigo = "bemvindo10" }).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Dado_CodigoVazio_Quando_Validar_Entao_DeveSerInvalido(string? codigo)
    {
        var resultado = _validator.Validate(new AplicacaoDeCupomDTO { Codigo = codigo! });

        var erro = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(AplicacaoDeCupomDTO.Codigo), erro.PropertyName);
        Assert.Equal("Informe o código do cupom.", erro.ErrorMessage);
    }

    [Fact]
    public void Dado_CodigoCom21Caracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(new AplicacaoDeCupomDTO { Codigo = new string('A', 21) });

        Assert.Equal("O código do cupom tem no máximo 20 caracteres.", Assert.Single(resultado.Errors).ErrorMessage);
    }
}
