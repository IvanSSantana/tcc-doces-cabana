using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class VotoUtilTests
{
    [Fact]
    public void Dado_DadosValidos_Quando_CriarVotoUtil_Entao_DeveRetornarVotoUtilInstanciado()
    {
        var avaliacaoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var voto = new VotoUtil(avaliacaoId, usuarioId);

        Assert.Equal(avaliacaoId, voto.AvaliacaoId);
        Assert.Equal(usuarioId, voto.UsuarioId);
    }

    [Fact]
    public void Dado_AvaliacaoInvalida_Quando_CriarVotoUtil_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VotoUtil(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarVotoUtil_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VotoUtil(Guid.NewGuid(), Guid.Empty));
    }
}
