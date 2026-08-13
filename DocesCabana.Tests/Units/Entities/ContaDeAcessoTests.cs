using DocesCabana.Infrastructure.Identity;

namespace DocesCabana.Tests.Units.Entities;

public class ContaDeAcessoTests
{
    [Fact]
    public void Dado_EmailValido_Quando_CriarInstancia_Entao_DeveRetornarContaValida()
    {
        var conta = new ContaDeAcesso("joao.silva@example.com");

        Assert.NotNull(conta);
        Assert.Equal("joao.silva@example.com", conta.Email);
        Assert.Equal("joao.silva@example.com", conta.UserName);
    }

    [Fact]
    public void Dado_EmailNulo_Quando_CriarInstancia_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ContaDeAcesso(""));
    }

    [Fact]
    public void Dado_EmailInvalido_Quando_CriarInstancia_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ContaDeAcesso("email_invalido"));
    }
}
