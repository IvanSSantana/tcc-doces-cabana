using System;
using DocesCabana.Infrastructure.Identity;
using Xunit;

namespace DocesCabana.Tests.Units.Entities;

public class UsuarioTests
{
    [Fact]
    public void Dado_UmUsuarioValido_Quando_CriarInstancia_Entao_DeveRetornarUsuarioValido()
    {
        var usuario = new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        );

        Assert.NotNull(usuario);
    }

    [Fact]
    public void Dado_NomeNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        ));
    }

    [Fact]
    public void Dado_EmailNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "João Silva",
            email: "",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        ));
    }

    [Fact]
    public void Dado_EmailInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "email_invalido",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        ));
    }

    [Fact]
    public void Dado_CelularNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        ));
    }

    [Fact]
    public void Dado_CelularInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "2322315342",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "54839427011"
        ));
    }

    [Fact]
    public void Dado_CpfNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: ""
        ));
    }

    [Fact]
    public void Dado_CpfInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "123.456.789-00"
        ));
    }

    [Fact]
    public void Dado_CpfComDigitosRepetidos_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "11111111111"
        ));
    }
}
