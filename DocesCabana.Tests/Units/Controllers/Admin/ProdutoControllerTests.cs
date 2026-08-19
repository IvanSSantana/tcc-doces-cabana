using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Enums;
using AdminProdutoController = DocesCabana.MVC.Areas.Admin.Controllers.ProdutoController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

// Era CatalogoControllerTests (010). Renomeado para acompanhar
// Areas/Admin/Controllers/ProdutoController (011, RQ-04). O alias evita
// colidir com o DocesCabana.MVC.Controllers.ProdutoController público, que
// já tem teste próprio em Units/Controllers/ProdutoControllerTests.cs.
public class ProdutoControllerTests
{
    private readonly Mock<IProdutoService> _produtoServiceMock;
    private readonly Mock<ISubcategoriaService> _subcategoriaServiceMock;
    private readonly AdminProdutoController _controller;

    public ProdutoControllerTests()
    {
        _produtoServiceMock = new Mock<IProdutoService>();
        _subcategoriaServiceMock = new Mock<ISubcategoriaService>();
        _controller = new AdminProdutoController(_produtoServiceMock.Object, _subcategoriaServiceMock.Object);
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveCarregarSubcategoriasERetornarView()
    {
        var subcategorias = new List<SubcategoriaDTO>
        {
            new() { SubcategoriaId = Guid.NewGuid(), Nome = "Doces de Tacho" }
        };
        _subcategoriaServiceMock.Setup(s => s.BuscarTodasSubcategorias())
            .ReturnsAsync(subcategorias);

        var resultado = await _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
        _subcategoriaServiceMock.Verify(s => s.BuscarTodasSubcategorias(), Times.Once);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarViewComDtoSemChamarServico()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new ProdutoDTO();
        _subcategoriaServiceMock.Setup(s => s.BuscarTodasSubcategorias())
            .ReturnsAsync(new List<SubcategoriaDTO>());

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ProdutoValido_Quando_CadastroPost_Entao_DeveCadastrarERedirecionarComConfirmacao()
    {
        var dto = new ProdutoDTO
        {
            Nome = "Brigadeiro Gourmet",
            Preco = 5.50m,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = "https://imagem.com/brigadeiro.jpg",
            SubcategoriaId = Guid.NewGuid()
        };
        _produtoServiceMock.Setup(s => s.Cadastrar(dto))
            .ReturnsAsync(dto);

        ConfigurarTempData();

        var resultado = await _controller.Cadastro(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Cadastro", redirectResult.ActionName);
        _produtoServiceMock.Verify(s => s.Cadastrar(dto), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    private void ConfigurarTempData()
    {
        var httpContext = new DefaultHttpContext();
        var tempDataProviderMock = new Mock<ITempDataProvider>();
        tempDataProviderMock.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());

        _controller.TempData = new TempDataDictionary(httpContext, tempDataProviderMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
