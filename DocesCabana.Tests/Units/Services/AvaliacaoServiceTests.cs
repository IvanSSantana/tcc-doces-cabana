using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class AvaliacaoServiceTests
{
    private readonly Mock<IAvaliacaoRepository> _avaliacaoRepositoryMock;
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AvaliacaoService _avaliacaoService;

    public AvaliacaoServiceTests()
    {
        _avaliacaoRepositoryMock = new Mock<IAvaliacaoRepository>();
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _avaliacaoService = new AvaliacaoService(
            _avaliacaoRepositoryMock.Object, _pedidoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Dado_AvaliacoesComNotasVariadas_Quando_ResumirPorProduto_Entao_DeveCalcularMediaComUmaCasa()
    {
        var produtoId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(3);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorNota(produtoId)).ReturnsAsync(
            new Dictionary<byte, int> { [5] = 2, [4] = 1 });

        var resumo = await _avaliacaoService.ResumirPorProduto(produtoId);

        Assert.Equal(4.7m, resumo.Media);
        Assert.Equal(3, resumo.Total);
    }

    [Fact]
    public async Task Dado_ProdutoSemAvaliacao_Quando_ResumirPorProduto_Entao_MediaDeveSerNula()
    {
        var produtoId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(0);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorNota(produtoId)).ReturnsAsync(
            new Dictionary<byte, int>());

        var resumo = await _avaliacaoService.ResumirPorProduto(produtoId);

        Assert.Null(resumo.Media);
        Assert.Equal(0, resumo.Total);
    }

    [Fact]
    public async Task Dado_DistribuicaoParcialDoRepositorio_Quando_ResumirPorProduto_Entao_DeveCompletarAsCincoChaves()
    {
        var produtoId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(5);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorNota(produtoId)).ReturnsAsync(
            new Dictionary<byte, int> { [5] = 3, [1] = 2 });

        var resumo = await _avaliacaoService.ResumirPorProduto(produtoId);

        Assert.Equal(5, resumo.Distribuicao.Count);
        for (byte nota = 1; nota <= 5; nota++)
            Assert.True(resumo.Distribuicao.ContainsKey(nota));

        Assert.Equal(3, resumo.Distribuicao[5]);
        Assert.Equal(0, resumo.Distribuicao[4]);
        Assert.Equal(0, resumo.Distribuicao[3]);
        Assert.Equal(0, resumo.Distribuicao[2]);
        Assert.Equal(2, resumo.Distribuicao[1]);
    }

    [Fact]
    public async Task Dado_OrdenacaoEscolhida_Quando_ListarPorProduto_Entao_DeveRepassarAoRepositorio()
    {
        var produtoId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(0);
        _avaliacaoRepositoryMock
            .Setup(r => r.BuscarPorProduto(produtoId, OrdenacaoAvaliacao.MaisRecentes, 5))
            .ReturnsAsync(Enumerable.Empty<Avaliacao>());

        await _avaliacaoService.ListarPorProduto(produtoId, OrdenacaoAvaliacao.MaisRecentes, 5, usuarioAtual: null);

        _avaliacaoRepositoryMock.Verify(
            r => r.BuscarPorProduto(produtoId, OrdenacaoAvaliacao.MaisRecentes, 5), Times.Once);
    }

    [Fact]
    public async Task Dado_OitoAvaliacoesEExibindoCinco_Quando_ListarPorProduto_Entao_TemMaisDeveSerVerdadeiro()
    {
        var produtoId = Guid.NewGuid();
        var cincoAvaliacoes = CriarAvaliacoes(produtoId, 5);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(8);
        _avaliacaoRepositoryMock
            .Setup(r => r.BuscarPorProduto(produtoId, OrdenacaoAvaliacao.Relevantes, 5))
            .ReturnsAsync(cincoAvaliacoes);

        var pagina = await _avaliacaoService.ListarPorProduto(produtoId, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null);

        Assert.Equal(5, pagina.Exibindo);
        Assert.Equal(8, pagina.Total);
        Assert.True(pagina.TemMais);
    }

    [Fact]
    public async Task Dado_OitoAvaliacoesEExibindoOito_Quando_ListarPorProduto_Entao_TemMaisDeveSerFalso()
    {
        var produtoId = Guid.NewGuid();
        var oitoAvaliacoes = CriarAvaliacoes(produtoId, 8);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(8);
        _avaliacaoRepositoryMock
            .Setup(r => r.BuscarPorProduto(produtoId, OrdenacaoAvaliacao.Relevantes, 10))
            .ReturnsAsync(oitoAvaliacoes);

        var pagina = await _avaliacaoService.ListarPorProduto(produtoId, OrdenacaoAvaliacao.Relevantes, 10, usuarioAtual: null);

        Assert.Equal(8, pagina.Exibindo);
        Assert.False(pagina.TemMais);
    }

    [Fact]
    public async Task Dado_UsuarioSemVoto_Quando_AlternarVotoUtil_Entao_DeveIncrementarTotalUteisESalvar()
    {
        var autor = Guid.NewGuid();
        var votante = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var avaliacao = new Avaliacao(autor, produtoId, 5);
        _avaliacaoRepositoryMock.Setup(r => r.BuscarComVotos(avaliacao.AvaliacaoId)).ReturnsAsync(avaliacao);

        var produtoIdRetornado = await _avaliacaoService.AlternarVotoUtil(avaliacao.AvaliacaoId, votante);

        Assert.Equal(produtoId, produtoIdRetornado);
        Assert.Equal(1, avaliacao.TotalUteis);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioJaVotou_Quando_AlternarVotoUtil_Entao_DeveRemoverOVoto()
    {
        var autor = Guid.NewGuid();
        var votante = Guid.NewGuid();
        var avaliacao = new Avaliacao(autor, Guid.NewGuid(), 5);
        avaliacao.AlternarVotoUtil(votante);
        _avaliacaoRepositoryMock.Setup(r => r.BuscarComVotos(avaliacao.AvaliacaoId)).ReturnsAsync(avaliacao);

        await _avaliacaoService.AlternarVotoUtil(avaliacao.AvaliacaoId, votante);

        Assert.Equal(0, avaliacao.TotalUteis);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_AutorDaAvaliacao_Quando_AlternarVotoUtil_Entao_DeveLancarInvalidOperationExceptionSemSalvar()
    {
        var autor = Guid.NewGuid();
        var avaliacao = new Avaliacao(autor, Guid.NewGuid(), 5);
        _avaliacaoRepositoryMock.Setup(r => r.BuscarComVotos(avaliacao.AvaliacaoId)).ReturnsAsync(avaliacao);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _avaliacaoService.AlternarVotoUtil(avaliacao.AvaliacaoId, autor));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    [Fact]
    public async Task Dado_AvaliacaoInexistente_Quando_AlternarVotoUtil_Entao_DeveLancarKeyNotFoundException()
    {
        _avaliacaoRepositoryMock.Setup(r => r.BuscarComVotos(It.IsAny<Guid>())).ReturnsAsync((Avaliacao?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _avaliacaoService.AlternarVotoUtil(Guid.NewGuid(), Guid.NewGuid()));
    }

    private static List<Avaliacao> CriarAvaliacoes(Guid produtoId, int quantidade) =>
        Enumerable.Range(1, quantidade)
            .Select(_ => new Avaliacao(Guid.NewGuid(), produtoId, 5))
            .ToList();

    [Fact]
    public async Task Dado_UsuarioQueJaAvaliouSemPedidoEntregue_Quando_VerificarSituacao_Entao_DeveSerJaAvaliou()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(true);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(false);

        var situacao = await _avaliacaoService.VerificarSituacao(produtoId, usuarioId);

        Assert.Equal(SituacaoDaAvaliacao.JaAvaliou, situacao);
    }

    [Fact]
    public async Task Dado_PedidoEntregueSemAvaliacao_Quando_VerificarSituacao_Entao_DeveSerPodeAvaliar()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(false);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(true);

        var situacao = await _avaliacaoService.VerificarSituacao(produtoId, usuarioId);

        Assert.Equal(SituacaoDaAvaliacao.PodeAvaliar, situacao);
    }

    [Fact]
    public async Task Dado_SemPedidoEntregue_Quando_VerificarSituacao_Entao_DeveSerNaoRecebeu()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(false);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(false);

        var situacao = await _avaliacaoService.VerificarSituacao(produtoId, usuarioId);

        Assert.Equal(SituacaoDaAvaliacao.NaoRecebeu, situacao);
    }

    private void ConfigurarUsuarioQuePodeAvaliar(Guid produtoId, Guid usuarioId)
    {
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(false);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(true);
    }

    private Func<Avaliacao?> CapturarAvaliacaoAdicionada()
    {
        Avaliacao? adicionada = null;
        _avaliacaoRepositoryMock
            .Setup(r => r.Adicionar(It.IsAny<Avaliacao>()))
            .Callback<Avaliacao>(a => adicionada = a)
            .Returns(Task.CompletedTask);
        return () => adicionada;
    }

    [Fact]
    public async Task Dado_UsuarioQuePodeAvaliar_Quando_Avaliar_Entao_DeveAdicionarESalvar()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioQuePodeAvaliar(produtoId, usuarioId);
        var adicionada = CapturarAvaliacaoAdicionada();

        await _avaliacaoService.Avaliar(
            new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 4, Comentario = "Delicioso" }, usuarioId);

        var avaliacao = adicionada();
        Assert.NotNull(avaliacao);
        Assert.Equal(usuarioId, avaliacao.UsuarioId);
        Assert.Equal(produtoId, avaliacao.ProdutoId);
        Assert.Equal((byte)4, avaliacao.Nota);
        Assert.Equal("Delicioso", avaliacao.Comentario);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Once);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Dado_ComentarioVazioOuSoDeEspacos_Quando_Avaliar_Entao_DeveGuardarNulo(string? comentario)
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioQuePodeAvaliar(produtoId, usuarioId);
        var adicionada = CapturarAvaliacaoAdicionada();

        await _avaliacaoService.Avaliar(
            new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 5, Comentario = comentario }, usuarioId);

        Assert.Null(adicionada()!.Comentario);
    }

    [Fact]
    public async Task Dado_ComentarioComEspacosNasPontas_Quando_Avaliar_Entao_DeveGuardarAparado()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioQuePodeAvaliar(produtoId, usuarioId);
        var adicionada = CapturarAvaliacaoAdicionada();

        await _avaliacaoService.Avaliar(
            new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 5, Comentario = "  Ótimo!  " }, usuarioId);

        Assert.Equal("Ótimo!", adicionada()!.Comentario);
    }

    [Fact]
    public async Task Dado_UsuarioQueJaAvaliou_Quando_Avaliar_Entao_DeveLancarInvalidOperationESemSalvar()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(true);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(true);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _avaliacaoService.Avaliar(new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 5 }, usuarioId));

        Assert.Equal("Você já comentou sobre esse produto.", excecao.Message);
        _avaliacaoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Avaliacao>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Never);
    }

    [Fact]
    public async Task Dado_UsuarioSemPedidoEntregue_Quando_Avaliar_Entao_DeveLancarInvalidOperationESemSalvar()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ExistePorUsuarioEProduto(usuarioId, produtoId)).ReturnsAsync(false);
        _pedidoRepositoryMock.Setup(r => r.ExisteEntregueComProduto(usuarioId, produtoId)).ReturnsAsync(false);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _avaliacaoService.Avaliar(new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 5 }, usuarioId));

        Assert.Equal("Só quem recebeu este produto pode avaliá-lo.", excecao.Message);
        _avaliacaoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Avaliacao>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Never);
    }

    [Fact]
    public async Task Dado_NotaNula_Quando_Avaliar_Entao_DeveLancarArgumentExceptionESemSalvar()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioQuePodeAvaliar(produtoId, usuarioId);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _avaliacaoService.Avaliar(new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = null }, usuarioId));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Never);
    }
}
