using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class ProdutoControllerTests
{
    private readonly Mock<IProdutoService> _produtoServiceMock;
    private readonly Mock<IAvaliacaoService> _avaliacaoServiceMock;
    private readonly ProdutoController _controller;

    public ProdutoControllerTests()
    {
        _produtoServiceMock = new Mock<IProdutoService>();
        _avaliacaoServiceMock = new Mock<IAvaliacaoService>();
        _controller = new ProdutoController(_produtoServiceMock.Object, _avaliacaoServiceMock.Object)
        {
            // Visitante anônimo por padrão; ConfigurarUsuarioAutenticado
            // substitui isto nos testes que precisam de um usuário logado.
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Dado_ProdutoExistente_Quando_Detalhes_Entao_DeveDevolverViewComODtoComposto()
    {
        var produtoId = Guid.NewGuid();
        var detalhe = new ProdutoDetalheDTO { ProdutoId = produtoId, Nome = "Brigadeiro" };
        _produtoServiceMock
            .Setup(s => s.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, 5, null))
            .ReturnsAsync(detalhe);

        var resultado = await _controller.Detalhes(produtoId);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(detalhe, viewResult.Model);
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    [InlineData(7, 5)]
    [InlineData(12, 10)]
    [InlineData(100, 100)]
    [InlineData(250, 100)]
    public async Task Dado_ExibirForaDaFaixaOuNaoMultiploDeCinco_Quando_Detalhes_Entao_DeveSanearAntesDeConsultar(int exibirRecebido, int exibirEsperado)
    {
        var produtoId = Guid.NewGuid();
        _produtoServiceMock
            .Setup(s => s.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, exibirEsperado, null))
            .ReturnsAsync(new ProdutoDetalheDTO { ProdutoId = produtoId });

        await _controller.Detalhes(produtoId, OrdenacaoAvaliacao.Relevantes, exibirRecebido);

        _produtoServiceMock.Verify(
            s => s.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, exibirEsperado, null), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_VotarUtil_Entao_DeveRedirecionarPreservandoOrdenacaoEExibir()
    {
        var usuarioId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        ConfigurarUrl();
        _avaliacaoServiceMock
            .Setup(s => s.AlternarVotoUtil(avaliacaoId, usuarioId))
            .ReturnsAsync(produtoId);

        var resultado = await _controller.VotarUtil(avaliacaoId, OrdenacaoAvaliacao.MaisRecentes, 10);

        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Contains("#avaliacoes", redirect.Url);
        _avaliacaoServiceMock.Verify(s => s.AlternarVotoUtil(avaliacaoId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Dado_AutorVotandoNaPropriaAvaliacao_Quando_VotarUtil_Entao_NaoDeveCapturarAExcecao()
    {
        // RF-21 / Princípio VIII: a ação não faz try/catch — quem decide o que
        // acontece na tela é o FilterException global, não o controller.
        var usuarioId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _avaliacaoServiceMock
            .Setup(s => s.AlternarVotoUtil(avaliacaoId, usuarioId))
            .ThrowsAsync(new InvalidOperationException("Você não pode marcar como útil a própria avaliação."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.VotarUtil(avaliacaoId, OrdenacaoAvaliacao.Relevantes, 5));
    }

    private void ConfigurarUsuarioAutenticado(Guid usuarioId)
    {
        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())], "TesteAutenticacao");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidade) };

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private void ConfigurarUrl()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("/Produto/Detalhes/00000000-0000-0000-0000-000000000000?ordenacao=MaisRecentes&exibir=10");

        _controller.Url = urlHelperMock.Object;
    }
}
