using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class EsqueceuSenhaDTOValidatorTests
{
    private readonly EsqueceuSenhaDTOValidator _validator = new();

    [Fact]
    public void Dado_LoginComEmailValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new EsqueceuSenhaDTO { Login = "usuario@exemplo.com" };

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_LoginComCpfValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new EsqueceuSenhaDTO { Login = "529.982.247-25" };

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_LoginVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new EsqueceuSenhaDTO { Login = "" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Login");
    }

    [Fact]
    public void Dado_LoginMalformado_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new EsqueceuSenhaDTO { Login = "abc" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Login");
    }
}
