using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class AvaliacaoTests
{
    private readonly Guid _usuarioValido = Guid.NewGuid();
    private readonly Guid _produtoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarAvaliacao_Entao_DeveRetornarAvaliacaoInstanciada()
    {
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5, "Muito bom!", true);

        Assert.Equal((byte)5, avaliacao.Nota);
        Assert.Equal("Muito bom!", avaliacao.Comentario);
        Assert.True(avaliacao.UpVote);
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarAvaliacao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Avaliacao(Guid.Empty, _produtoValido, 5));
    }

    [Fact]
    public void Dado_ProdutoInvalido_Quando_CriarAvaliacao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Avaliacao(_usuarioValido, Guid.Empty, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Dado_NotaForaDaFaixa_Quando_CriarAvaliacao_Entao_DeveLancarArgumentException(byte nota)
    {
        Assert.Throws<ArgumentException>(() => new Avaliacao(_usuarioValido, _produtoValido, nota));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Dado_NotaNoLimiteDaFaixa_Quando_CriarAvaliacao_Entao_DeveConstruir(byte nota)
    {
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, nota);

        Assert.Equal(nota, avaliacao.Nota);
    }

    [Fact]
    public void Dado_ComentarioNulo_Quando_CriarAvaliacao_Entao_DeveAceitar()
    {
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5, comentario: null);

        Assert.Null(avaliacao.Comentario);
    }

    [Fact]
    public void Dado_ComentarioComMaisDe255Caracteres_Quando_CriarAvaliacao_Entao_DeveLancarArgumentException()
    {
        var comentario = new string('a', 256);

        Assert.Throws<ArgumentException>(() => new Avaliacao(_usuarioValido, _produtoValido, 5, comentario));
    }
}
