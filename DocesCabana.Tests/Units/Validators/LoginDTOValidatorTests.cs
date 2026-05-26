using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Validators;
using Xunit;

namespace DocesCabana.Tests.Units.Validators;

public class LoginDTOValidatorTests
{
    private readonly LoginDTOValidator _validator = new();

    [Fact]
    public void Dado_EmailValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new LoginDTO { Login = "usuario@exemplo.com", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_CpfValidoLimpo_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new LoginDTO { Login = "54839427011", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_CpfValidoFormatado_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = new LoginDTO { Login = "548.394.270-11", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_NumeroDeTelefone_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new LoginDTO { Login = "11987654321", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Login");
    }

    [Fact]
    public void Dado_CpfInvalidoRepetido_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new LoginDTO { Login = "11111111111", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Login");
    }

    [Fact]
    public void Dado_LoginNulo_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = new LoginDTO { Login = "", Senha = "SenhaForte@123" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Login");
    }
}
