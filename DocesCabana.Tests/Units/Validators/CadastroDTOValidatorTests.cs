using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class CadastroDTOValidatorTests
{
    private readonly CadastroDTOValidator _validator = new();

    [Fact]
    public void Dado_UmCadastroValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarCadastroValido();

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_NomeNulo_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarCadastroValido();
        dto.Nome = "";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Dado_EmailInvalido_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarCadastroValido();
        dto.Email = "email_invalido";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Dado_CelularInvalido_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarCadastroValido();
        dto.Celular = "123456";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Celular");
    }

    [Fact]
    public void Dado_CpfInvalido_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarCadastroValido();
        dto.CPF = "123.456.78"; // Menos de 11 dígitos, inválido para o helper

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "CPF");
    }

    [Fact]
    public void Dado_SenhasDiferentes_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarCadastroValido();
        dto.ConfirmacaoSenha = "SenhaDiferente@123";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmacaoSenha");
    }

    private CadastroDTO CriarCadastroValido()
    {
        return new CadastroDTO
        {
            Nome = "João Silva",
            Email = "joao.silva@example.com",
            Celular = "11987654321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "54839427011",
            Senha = "SenhaForte@123",
            ConfirmacaoSenha = "SenhaForte@123"
        };
    }
}
