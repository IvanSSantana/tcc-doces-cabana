using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class EnderecoDTOValidatorTests
{
    private readonly EnderecoDTOValidator _validator = new();

    private static EnderecoDTO CriarEnderecoValido(
        string? estado = "São Paulo", string? cidade = "Barra Bonita", string? bairro = "Centro",
        string? cep = "17340-000", string? rua = "Rua das Flores", int numero = 123) => new()
    {
        Estado = estado!,
        Cidade = cidade!,
        Bairro = bairro!,
        CEP = cep!,
        Rua = rua!,
        Numero = numero,
    };

    [Fact]
    public void Dado_UmEnderecoValido_Quando_Validar_Entao_DeveSerValido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_EstadoVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido(estado: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Estado");
    }

    [Fact]
    public void Dado_CidadeVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido(cidade: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Cidade");
    }

    [Fact]
    public void Dado_BairroVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido(bairro: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Bairro");
    }

    [Fact]
    public void Dado_RuaVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido(rua: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Rua");
    }

    [Fact]
    public void Dado_CepVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarEnderecoValido(cep: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "CEP");
    }

    [Theory]
    [InlineData("1734000")]
    [InlineData("173400000")]
    public void Dado_CepComQuantidadeDeDigitosInvalida_Quando_Validar_Entao_DeveSerInvalido(string cep)
    {
        var resultado = _validator.Validate(CriarEnderecoValido(cep: cep));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "CEP");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_NumeroInvalido_Quando_Validar_Entao_DeveSerInvalido(int numero)
    {
        var resultado = _validator.Validate(CriarEnderecoValido(numero: numero));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Numero");
    }
}
