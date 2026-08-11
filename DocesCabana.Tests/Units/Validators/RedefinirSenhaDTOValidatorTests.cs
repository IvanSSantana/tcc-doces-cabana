using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class RedefinirSenhaDTOValidatorTests
{
    private readonly RedefinirSenhaDTOValidator _validator = new();

    [Fact]
    public void Dado_UmaSenhaValida_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarRedefinicaoValida();

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_SenhaVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = "";
        dto.ConfirmacaoSenha = "";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_SenhaSemMaiuscula_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = dto.ConfirmacaoSenha = "senha@123";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_SenhaSemMinuscula_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = dto.ConfirmacaoSenha = "SENHA@123";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_SenhaSemNumero_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = dto.ConfirmacaoSenha = "SenhaForte@";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_SenhaSemCaractereEspecial_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = dto.ConfirmacaoSenha = "SenhaForte123";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_SenhaComMenosDeSeisCaracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.Senha = dto.ConfirmacaoSenha = "Sn@1";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Dado_ConfirmacaoVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.ConfirmacaoSenha = "";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmacaoSenha");
    }

    [Fact]
    public void Dado_ConfirmacaoDiferenteDaSenha_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarRedefinicaoValida();
        dto.ConfirmacaoSenha = "SenhaDiferente@123";

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmacaoSenha");
    }

    private static RedefinirSenhaDTO CriarRedefinicaoValida() =>
        new()
        {
            Token = "token-teste",
            Email = "teste@exemplo.com",
            Senha = "SenhaForte@123",
            ConfirmacaoSenha = "SenhaForte@123"
        };
}
