using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class AvaliacaoTests
{
    private readonly Guid _usuarioValido = Guid.NewGuid();
    private readonly Guid _produtoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarAvaliacao_Entao_DeveRetornarAvaliacaoInstanciada()
    {
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5, "Muito bom!");

        Assert.Equal((byte)5, avaliacao.Nota);
        Assert.Equal("Muito bom!", avaliacao.Comentario);
    }

    [Fact]
    public void Dado_AvaliacaoRecemCriada_Quando_CriarAvaliacao_Entao_DataCriacaoDeveSerPreenchida()
    {
        // RN-09: toda avaliação registra a data em que foi escrita.
        var antes = DateTime.UtcNow;

        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5);

        var depois = DateTime.UtcNow;
        Assert.InRange(avaliacao.DataCriacao, antes, depois);
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

    [Fact]
    public void Dado_AvaliacaoSemVotos_Quando_AlternarVotoUtil_Entao_DeveMarcarEIncrementarTotalUteis()
    {
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5);
        var votante = Guid.NewGuid();

        var marcou = avaliacao.AlternarVotoUtil(votante);

        Assert.True(marcou);
        Assert.Equal(1, avaliacao.TotalUteis);
        Assert.True(avaliacao.MarcadaComoUtilPor(votante));
    }

    [Fact]
    public void Dado_VotoJaMarcado_Quando_AlternarVotoUtil_Entao_DeveDesmarcarEDecrementarTotalUteis()
    {
        // RN-06: uma pessoa marca no máximo uma vez; marcar de novo desfaz.
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5);
        var votante = Guid.NewGuid();
        avaliacao.AlternarVotoUtil(votante);

        var marcou = avaliacao.AlternarVotoUtil(votante);

        Assert.False(marcou);
        Assert.Equal(0, avaliacao.TotalUteis);
        Assert.False(avaliacao.MarcadaComoUtilPor(votante));
    }

    [Fact]
    public void Dado_AutorDaAvaliacao_Quando_AlternarVotoUtil_Entao_DeveLancarInvalidOperationException()
    {
        // RN-07: ninguém marca como útil a própria avaliação.
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5);

        Assert.Throws<InvalidOperationException>(() => avaliacao.AlternarVotoUtil(_usuarioValido));
    }

    [Fact]
    public void Dado_VariosVotantesDistintos_Quando_AlternarVotoUtil_Entao_TotalUteisContaCadaUmUmaVezENuncaFicaNegativo()
    {
        // RN-08: contagem de pessoas distintas, nunca negativa.
        var avaliacao = new Avaliacao(_usuarioValido, _produtoValido, 5);
        var votanteUm = Guid.NewGuid();
        var votanteDois = Guid.NewGuid();

        avaliacao.AlternarVotoUtil(votanteUm);
        avaliacao.AlternarVotoUtil(votanteDois);

        Assert.Equal(2, avaliacao.TotalUteis);

        avaliacao.AlternarVotoUtil(votanteUm);

        Assert.Equal(1, avaliacao.TotalUteis);
        Assert.True(avaliacao.TotalUteis >= 0);
    }
}
