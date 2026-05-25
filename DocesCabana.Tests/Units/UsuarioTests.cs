using DocesCabana.Infrastructure.Identity;

namespace DocesCabana.Units.Tests;

public class UsuarioTests
{
    [Fact]
    public void Inserir_Usuario_Valido_Retorna_Usuario()
    {
        var usuario = new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        );

        Assert.NotNull(usuario);
    }

    [Fact]
    public void Inserir_Nome_Nulo_Lanca_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        ));
    }

    [Fact]
    public void Inserir_Email_Nulo_Lanca_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "João Silva",
            email: "",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        ));
    }

    [Fact]
    public void Inserir_Email_Invalido_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "email_invalido",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        ));
    }

    [Fact]
    public void Inserir_Celular_Nulo_Lanca_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        ));
    }

    [Fact]
    public void Inserir_Celular_Invalido_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "2322315342",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "548.394.270-11"
        ));
    }

    [Fact]
    public void Inserir_CPF_Nulo_Lanca_ArgumentNullException()
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
    public void Inserir_CPF_Invalido_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            nome: "João Silva",
            email: "joao.silva@example.com",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1),
            cpf: "123.456.789-00"
        ));
    }
}