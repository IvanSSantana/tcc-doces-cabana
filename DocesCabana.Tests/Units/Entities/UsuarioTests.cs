using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class UsuarioTests
{
    private readonly Guid _usuarioIdValido = Guid.NewGuid();

    [Fact]
    public void Dado_UmUsuarioValido_Quando_CriarInstancia_Entao_DeveRetornarUsuarioValido()
    {
        var usuario = new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        );

        Assert.NotNull(usuario);
        Assert.Equal(_usuarioIdValido, usuario.UsuarioId);
    }

    [Fact]
    public void Dado_UsuarioIdVazio_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: Guid.Empty,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_NomeNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "",
            cpf: "54839427011",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CelularNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CelularInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "2322315342",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CelularPontuado_Quando_CriarInstancia_Entao_DeveNormalizarParaDigitos()
    {
        var usuario = new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "(11) 98765-4321",
            dataNascimento: new DateTime(1990, 1, 1)
        );

        Assert.Equal("11987654321", usuario.Celular);
    }

    [Fact]
    public void Dado_CpfNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CpfInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "123.456.789-00",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CpfComDigitosRepetidos_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "11111111111",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        ));
    }

    [Fact]
    public void Dado_CpfPontuado_Quando_CriarInstancia_Entao_DeveNormalizarParaDigitos()
    {
        var usuario = new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "548.394.270-11",
            celular: "11987654321",
            dataNascimento: new DateTime(1990, 1, 1)
        );

        Assert.Equal("54839427011", usuario.CPF);
    }

    [Fact]
    public void Dado_DataNascimentoFutura_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "11987654321",
            dataNascimento: DateTime.Today.AddDays(1)
        ));
    }

    [Fact]
    public void Dado_DataNascimentoAnteriorA120Anos_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            usuarioId: _usuarioIdValido,
            nome: "João Silva",
            cpf: "54839427011",
            celular: "11987654321",
            dataNascimento: DateTime.Today.AddYears(-121)
        ));
    }

    [Fact]
    public void Dado_NovosDadosValidos_Quando_AtualizarDados_Entao_DeveAtualizarNomeCelularEDataNascimento()
    {
        var usuario = new Usuario(_usuarioIdValido, "João Silva", "54839427011", "11987654321", new DateTime(1990, 1, 1));

        usuario.AtualizarDados("João Pedro Silva", "(11) 98888-8888", new DateTime(1991, 2, 2));

        Assert.Equal("João Pedro Silva", usuario.Nome);
        Assert.Equal("11988888888", usuario.Celular);
        Assert.Equal(new DateTime(1991, 2, 2), usuario.DataNascimento);
    }
}
