using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class DadosPessoaisDTOValidatorTests
{
    private readonly DadosPessoaisDTOValidator _validator = new();

    private static DadosPessoaisDTO CriarValido(
        string? nome = "Cliente Teste", string? celular = "(14) 99999-9999", DateTime? dataNascimento = null) => new()
    {
        Nome = nome!,
        Celular = celular!,
        DataNascimento = dataNascimento ?? new DateTime(1994, 6, 6),
        CPF = "52998224725",
    };

    [Fact]
    public void Dado_DadosPessoaisValidos_Quando_Validar_Entao_DeveSerValido()
    {
        var resultado = _validator.Validate(CriarValido());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_NomeVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarValido(nome: ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Dado_CelularInvalido_Quando_Validar_Entao_DeveSerInvalido()
    {
        var resultado = _validator.Validate(CriarValido(celular: "123"));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Celular");
    }

    [Fact]
    public void Dado_DataDeNascimentoVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.DataNascimento = default;

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "DataNascimento");
    }

    // Plano §9, risco 7: DadosPessoaisDTOValidator reaproveita as regras de
    // CadastroDTOValidator em vez de reescrevê-las — este teste prova que os
    // dois concordam para o mesmo valor inválido, em vez de assumir.
    [Theory]
    [InlineData("123")]
    [InlineData("")]
    public void Dado_CelularInvalido_Quando_ValidarNosDoisValidators_Entao_DevemConcordar(string celular)
    {
        var resultadoConta = _validator.Validate(CriarValido(celular: celular));
        var resultadoCadastro = new CadastroDTOValidator().Validate(new CadastroDTO
        {
            Nome = "Cliente Teste",
            Email = "cliente@teste.com",
            Celular = celular,
            DataNascimento = new DateTime(1994, 6, 6),
            CPF = "529.982.247-25",
            Senha = "Senha@123",
            ConfirmacaoSenha = "Senha@123",
        });

        Assert.Equal(
            resultadoCadastro.Errors.Any(e => e.PropertyName == "Celular"),
            resultadoConta.Errors.Any(e => e.PropertyName == "Celular"));
    }

    [Fact]
    public void Dado_DataDeNascimentoVazia_Quando_ValidarNosDoisValidators_Entao_DevemConcordar()
    {
        var dtoInvalido = CriarValido();
        dtoInvalido.DataNascimento = default;
        var resultadoConta = _validator.Validate(dtoInvalido);
        var resultadoCadastro = new CadastroDTOValidator().Validate(new CadastroDTO
        {
            Nome = "Cliente Teste",
            Email = "cliente@teste.com",
            Celular = "(14) 99999-9999",
            DataNascimento = null,
            CPF = "529.982.247-25",
            Senha = "Senha@123",
            ConfirmacaoSenha = "Senha@123",
        });

        Assert.Equal(
            resultadoCadastro.Errors.Any(e => e.PropertyName == "DataNascimento"),
            resultadoConta.Errors.Any(e => e.PropertyName == "DataNascimento"));
    }
}
