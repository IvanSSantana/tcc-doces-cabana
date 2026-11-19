using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Models;
using AdminPromocaoController = DocesCabana.MVC.Areas.Admin.Controllers.PromocaoController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class PromocaoControllerTests
{
    private readonly Mock<IPromocaoService> _promocaoServiceMock = new();
    private readonly Mock<ICategoriaService> _categoriaServiceMock = new();
    private readonly Mock<IProdutoService> _produtoServiceMock = new();
    private readonly AdminPromocaoController _controller;

    private readonly Guid _subcategoriaId = Guid.NewGuid();

    public PromocaoControllerTests()
    {
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(
        [
            new()
            {
                CategoriaId = Guid.NewGuid(),
                Nome = "Doces",
                Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = _subcategoriaId, Nome = "Brigadeiros" }]
            }
        ]);
        _produtoServiceMock.Setup(s => s.BuscarTodosProdutos()).ReturnsAsync(
        [
            new() { ProdutoId = Guid.NewGuid(), Nome = "Brigadeiro", SubcategoriaId = _subcategoriaId, Status = ProdutoStatus.Ativo },
            new() { ProdutoId = Guid.NewGuid(), Nome = "Beijinho", SubcategoriaId = _subcategoriaId, Status = ProdutoStatus.ForaDeEstoque },
        ]);

        var contextoHttp = new DefaultHttpContext();
        _controller = new AdminPromocaoController(
            _promocaoServiceMock.Object, _categoriaServiceMock.Object, _produtoServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, Mock.Of<ITempDataProvider>())
        };
    }

    private static PromocaoDTO DtoValido() => new()
    {
        Nome = "Semana do doce",
        Tipo = TipoDeDesconto.Percentual,
        Valor = 10,
        DataInicio = new DateOnly(2026, 11, 20),
        DataFim = new DateOnly(2026, 11, 25),
        Abrangencia = AbrangenciaDaPromocao.Subcategoria,
        SubcategoriaId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Dado_PromocoesCadastradas_Quando_Index_Entao_DeveRetornarViewComALista()
    {
        _promocaoServiceMock.Setup(s => s.ListarParaGestao()).ReturnsAsync([]);

        var resultado = await _controller.Index();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.IsAssignableFrom<IReadOnlyList<ResumoDePromocaoDTO>>(view.Model);
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveOferecerSoProdutosAtivos()
    {
        var resultado = await _controller.Cadastro();

        var view = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<FormularioDePromocaoViewModel>(view.Model);
        Assert.Equal("Brigadeiro", Assert.Single(modelo.Produtos).Nome);
        Assert.Single(modelo.Categorias);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_Cadastro_Entao_DeveRetornarViewComOpcoes()
    {
        _controller.ModelState.AddModelError("Promocao.Nome", "Nome é obrigatório!");

        var resultado = await _controller.Cadastro(DtoValido());

        var view = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<FormularioDePromocaoViewModel>(view.Model);
        Assert.NotEmpty(modelo.Categorias);
        Assert.NotEmpty(modelo.Produtos);
        _promocaoServiceMock.Verify(s => s.Cadastrar(It.IsAny<PromocaoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PromocaoValida_Quando_Cadastro_Entao_DeveRedirecionarParaIndex()
    {
        var dto = DtoValido();

        var resultado = await _controller.Cadastro(dto);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(AdminPromocaoController.Index), redirecionamento.ActionName);
        _promocaoServiceMock.Verify(s => s.Cadastrar(dto), Times.Once);
    }

    [Fact]
    public async Task Dado_Id_Quando_EdicaoGet_Entao_DeveRetornarViewComAPromocao()
    {
        var dto = DtoValido();
        dto.PromocaoId = Guid.NewGuid();
        _promocaoServiceMock.Setup(s => s.BuscarParaEdicao(dto.PromocaoId)).ReturnsAsync(dto);

        var resultado = await _controller.Edicao(dto.PromocaoId);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Same(dto, Assert.IsType<FormularioDePromocaoViewModel>(view.Model).Promocao);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_Edicao_Entao_DeveRetornarViewSemAtualizar()
    {
        _controller.ModelState.AddModelError("Promocao.DataFim", "A data de fim não pode ser anterior à de início.");

        var resultado = await _controller.Edicao(DtoValido());

        Assert.IsType<ViewResult>(resultado);
        _promocaoServiceMock.Verify(s => s.Atualizar(It.IsAny<PromocaoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PromocaoValida_Quando_Edicao_Entao_DeveAtualizarERedirecionar()
    {
        var dto = DtoValido();

        var resultado = await _controller.Edicao(dto);

        Assert.IsType<RedirectToActionResult>(resultado);
        _promocaoServiceMock.Verify(s => s.Atualizar(dto), Times.Once);
    }

    [Fact]
    public async Task Dado_Id_Quando_Desativar_Entao_DeveChamarServicoERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Desativar(id);

        Assert.IsType<RedirectToActionResult>(resultado);
        _promocaoServiceMock.Verify(s => s.Desativar(id), Times.Once);
    }

    [Fact]
    public async Task Dado_Id_Quando_Ativar_Entao_DeveChamarServicoERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Ativar(id);

        Assert.IsType<RedirectToActionResult>(resultado);
        _promocaoServiceMock.Verify(s => s.Ativar(id), Times.Once);
    }
}
