using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class ConsultaDeFreteDTOValidatorTests
{
    private readonly ConsultaDeFreteDTOValidator _validator = new();

    // RF-09/CA-10 (spec 020): CEP com formato inválido é recusado **antes**
    // de qualquer consulta ao serviço de entrega — é a barreira de entrada
    // que torna o CA-10 verificável sem rede.
    [Fact]
    public void Dado_CepVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new ConsultaDeFreteDTO(Cep: "");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Cep");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefgh")]
    [InlineData("123456789")]
    public void Dado_CepComFormatoInvalido_Quando_Validar_Entao_DeveSerInvalido(string cep)
    {
        var dto = new ConsultaDeFreteDTO(cep);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Cep");
    }

    [Fact]
    public void Dado_CepValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new ConsultaDeFreteDTO("01310000");

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_CepPontuado_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new ConsultaDeFreteDTO("01310-000");

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }
}
