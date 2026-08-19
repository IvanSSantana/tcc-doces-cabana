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
    private readonly Mock<ICategoriaService> _categoriaServiceMock;
    private readonly AdminProdutoController _controller;

    public ProdutoControllerTests()
    {
        _produtoServiceMock = new Mock<IProdutoService>();
        _categoriaServiceMock = new Mock<ICategoriaService>();
        _controller = new AdminProdutoController(_produtoServiceMock.Object, _categoriaServiceMock.Object);
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveCarregarSubcategoriasERetornarView()
    {
        var categorias = new List<CategoriaDTO>
        {
            new()
            {
                CategoriaId = Guid.NewGuid(),
                Nome = "Doces",
                Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = Guid.NewGuid(), Nome = "Barras" }]
            }
        };
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(categorias);

        var resultado = await _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
        _categoriaServiceMock.Verify(s => s.ListarComSubcategorias(), Times.Once);
    }

    [Fact]
    public async Task Dado_SubcategoriaComMesmoNomeEmDuasCategorias_Quando_CadastroGet_Entao_DeveQualificarPelaCategoria()
    {
        // RF-28/CA-24: "Cappuccino" existe em Doces e em Empório. O seletor
        // precisa distinguir as duas, não listar "Cappuccino" duas vezes.
        var idCappuccinoDoces = Guid.NewGuid();
        var idCappuccinoEmporio = Guid.NewGuid();
        var categorias = new List<CategoriaDTO>
        {
            new() { CategoriaId = Guid.NewGuid(), Nome = "Doces", Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = idCappuccinoDoces, Nome = "Cappuccino" }] },
            new() { CategoriaId = Guid.NewGuid(), Nome = "Empório", Apelido = "emporio",
                Subcategorias = [new() { SubcategoriaId = idCappuccinoEmporio, Nome = "Cappuccino" }] },
        };
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(categorias);

        var resultado = await _controller.Cadastro();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var opcoes = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.Rendering.SelectList>(viewResult.ViewData["Subcategorias"]);
        var rotulos = opcoes.Cast<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>().Select(i => i.Text).ToList();

        Assert.Contains("Doces › Cappuccino", rotulos);
        Assert.Contains("Empório › Cappuccino", rotulos);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarViewComDtoSemChamarServico()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new ProdutoDTO();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

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
