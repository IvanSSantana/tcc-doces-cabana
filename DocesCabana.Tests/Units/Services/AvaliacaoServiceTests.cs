using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class AvaliacaoServiceTests
{
    private readonly Mock<IAvaliacaoRepository> _avaliacaoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AvaliacaoService _avaliacaoService;

    public AvaliacaoServiceTests()
    {
        _avaliacaoRepositoryMock = new Mock<IAvaliacaoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _avaliacaoService = new AvaliacaoService(_avaliacaoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Dado_AvaliacoesComNotasVariadas_Quando_ResumirPorProduto_Entao_DeveCalcularMediaComUmaCasa()
    {
        // RN-03: média aritmética arredondada para uma casa. 4,5+4,5+5+4 / 3 -> na
        // prática usamos a distribuição por nota, que é o que o repositório expõe.
        var produtoId = Guid.NewGuid();
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorProduto(produtoId)).ReturnsAsync(3);
        _avaliacaoRepositoryMock.Setup(r => r.ContarPorNota(produtoId)).ReturnsAsync(
            new Dictionary<byte, int> { [5] = 2, [4] = 1 });

        var resumo = await _avaliacaoService.ResumirPorProduto(produtoId);

        // (5*2 + 4*1) / 3 = 4,666... -> 4,7
        Assert.Equal(4.7m, resumo.Media);
        Assert.Equal(3, resumo.Total);
    }

    [Fact]
    public async Task Dado_ProdutoSemAvaliacao_Quando_ResumirPorProduto_Entao_MediaDeveSerNula()
    {
        // CA-08: produto sem avaliação não tem média — não é zero.
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
        // RN-04: cada faixa é proporção sobre o total; a distribuição sempre
        // tem as cinco chaves, mesmo quando o repositório só devolve as notas
        // que de fato existem.
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
        // RF-14, CA-09
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
        // RF-15, CA-09
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
        // CA-14: um envio forçado do voto na própria avaliação não altera nada.
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
}
