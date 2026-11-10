using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Constants;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

    [Fact]
    public async Task Dado_RequisicaoAjaxDeRedirecionamento_Quando_Detalhes_Entao_DeveRetornarNoContentSemConsultar()
    {
        var produtoId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var resultado = await _controller.Detalhes(produtoId);

        Assert.IsType<NoContentResult>(resultado);
        _produtoServiceMock.Verify(
            s => s.BuscarDetalhe(It.IsAny<Guid>(), It.IsAny<OrdenacaoAvaliacao>(), It.IsAny<int>(), It.IsAny<Guid?>()),
            Times.Never);
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
        var usuarioId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _avaliacaoServiceMock
            .Setup(s => s.AlternarVotoUtil(avaliacaoId, usuarioId))
            .ThrowsAsync(new InvalidOperationException("Você não pode marcar como útil a própria avaliação."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.VotarUtil(avaliacaoId, OrdenacaoAvaliacao.Relevantes, 5));
    }

    [Fact]
    public async Task Dado_Administrador_Quando_Avaliar_Entao_DeveRetornarForbidSemChamarServico()
    {
        ConfigurarUsuarioAutenticado(Guid.NewGuid(), administrador: true);

        var resultado = await _controller.Avaliar(new NovaAvaliacaoDTO { ProdutoId = Guid.NewGuid(), Nota = 5 });

        Assert.IsType<ForbidResult>(resultado);
        _avaliacaoServiceMock.Verify(
            s => s.Avaliar(It.IsAny<NovaAvaliacaoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_Avaliar_Entao_DeveRetornarParcialComStatus400()
    {
        ConfigurarUsuarioAutenticado(Guid.NewGuid());
        var dto = new NovaAvaliacaoDTO { ProdutoId = Guid.NewGuid() };
        _controller.ModelState.AddModelError(nameof(NovaAvaliacaoDTO.Nota), "Escolha uma nota de 1 a 5.");

        var resultado = await _controller.Avaliar(dto);

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, parcial.StatusCode);
        Assert.Equal("_FormularioAvaliacao", parcial.ViewName);
        Assert.Same(dto, parcial.Model);
        _avaliacaoServiceMock.Verify(
            s => s.Avaliar(It.IsAny<NovaAvaliacaoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AvaliacaoValida_Quando_Avaliar_Entao_DeveChamarServicoERedirecionarParaMaisRecentes()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var dto = new NovaAvaliacaoDTO { ProdutoId = produtoId, Nota = 5, Comentario = "Ótimo" };
        ConfigurarUsuarioAutenticado(usuarioId);
        ConfigurarTempData();
        UrlActionContext? contextoDaUrl = null;
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Callback<UrlActionContext>(c => contextoDaUrl = c)
            .Returns("/Produto/Detalhes/x");
        _controller.Url = urlHelperMock.Object;

        var resultado = await _controller.Avaliar(dto);

        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.EndsWith("#avaliacoes", redirect.Url);
        Assert.NotNull(contextoDaUrl);
        Assert.Equal(nameof(ProdutoController.Detalhes), contextoDaUrl!.Action);
        Assert.Contains("MaisRecentes", contextoDaUrl.Values!.ToString());
        Assert.Equal("Obrigado pela sua avaliação!", _controller.TempData["ConfirmacaoAvaliacao"]);
        _avaliacaoServiceMock.Verify(s => s.Avaliar(dto, usuarioId), Times.Once);
    }

    private void ConfigurarUsuarioAutenticado(Guid usuarioId, bool administrador = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, usuarioId.ToString()) };
        if (administrador)
            claims.Add(new Claim(ClaimTypes.Role, Papeis.Administrador));

        var identidade = new ClaimsIdentity(claims, "TesteAutenticacao");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidade) };

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private void ConfigurarTempData()
    {
        var tempDataProviderMock = new Mock<ITempDataProvider>();
        tempDataProviderMock.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());

        _controller.TempData = new TempDataDictionary(_controller.HttpContext, tempDataProviderMock.Object);
    }

    private void ConfigurarUrl()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("/Produto/Detalhes/00000000-0000-0000-0000-000000000000?ordenacao=MaisRecentes&exibir=10");

        _controller.Url = urlHelperMock.Object;
    }
}
