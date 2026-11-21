using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Constants;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Models;
using AdminCupomController = DocesCabana.MVC.Areas.Admin.Controllers.CupomController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class CupomControllerTests
{
    private readonly Mock<ICupomService> _cupomServiceMock = new();
    private readonly Mock<ICategoriaService> _categoriaServiceMock = new();
    private readonly AdminCupomController _controller;

    public CupomControllerTests()
    {
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(
        [
            new()
            {
                CategoriaId = Guid.NewGuid(),
                Nome = "Doces",
                Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = Guid.NewGuid(), Nome = "Brigadeiros" }]
            }
        ]);

        var contextoHttp = new DefaultHttpContext();
        _controller = new AdminCupomController(_cupomServiceMock.Object, _categoriaServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, Mock.Of<ITempDataProvider>())
        };
    }

    private static CupomDTO DtoValido() => new()
    {
        Codigo = "FRETE50",
        Tipo = TipoDeDesconto.FreteGratis,
        DataInicio = new DateOnly(2026, 11, 20),
        DataFim = new DateOnly(2026, 12, 20),
        ValorMinimo = 50,
    };

    [Fact]
    public void Dado_Controller_Quando_InspecionarAtributos_Entao_DeveExigirPapelDeAdministrador()
    {
        var autorizacao = typeof(AdminCupomController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(Papeis.Administrador, autorizacao.Roles);
    }

    [Fact]
    public async Task Dado_CuponsCadastrados_Quando_Index_Entao_DeveDevolverAListagem()
    {
        IReadOnlyList<ResumoDeCupomDTO> cupons = [];
        _cupomServiceMock.Setup(s => s.ListarParaGestao()).ReturnsAsync(cupons);

        var resultado = await _controller.Index();

        Assert.Same(cupons, Assert.IsType<ViewResult>(resultado).Model);
    }

    [Fact]
    public async Task Dado_Requisicao_Quando_CadastroGet_Entao_DeveDevolverFormularioNovoComCategorias()
    {
        var resultado = await _controller.Cadastro();

        var modelo = Assert.IsType<FormularioDeCupomViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.True(modelo.EhNovo);
        Assert.Equal(TipoDeDesconto.Percentual, modelo.Cupom.Tipo);
        Assert.Single(modelo.Categorias);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_Cadastro_Entao_DeveCadastrarERedirecionar()
    {
        var dto = DtoValido();

        var resultado = await _controller.Cadastro(dto);

        _cupomServiceMock.Verify(s => s.Cadastrar(dto), Times.Once);
        Assert.Equal(nameof(AdminCupomController.Index), Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal("Cupom cadastrado com sucesso!", _controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_Cadastro_Entao_DeveVoltarAoFormularioSemCadastrar()
    {
        _controller.ModelState.AddModelError("Cupom.Codigo", "Código é obrigatório!");

        var resultado = await _controller.Cadastro(DtoValido());

        Assert.IsType<FormularioDeCupomViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        _cupomServiceMock.Verify(s => s.Cadastrar(It.IsAny<CupomDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CodigoExistente_Quando_Cadastro_Entao_DeveRetornarViewComErroEmCodigo()
    {
        _cupomServiceMock.Setup(s => s.CodigoJaExiste("FRETE50")).ReturnsAsync(true);

        var resultado = await _controller.Cadastro(DtoValido());

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Já existe um cupom com este código.",
            _controller.ModelState["Cupom.Codigo"]!.Errors.Single().ErrorMessage);
        _cupomServiceMock.Verify(s => s.Cadastrar(It.IsAny<CupomDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CupomExistente_Quando_EdicaoGet_Entao_DeveDevolverFormularioPreenchido()
    {
        var dto = DtoValido();
        dto.CupomId = Guid.NewGuid();
        _cupomServiceMock.Setup(s => s.BuscarParaEdicao(dto.CupomId)).ReturnsAsync(dto);

        var resultado = await _controller.Edicao(dto.CupomId);

        var modelo = Assert.IsType<FormularioDeCupomViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.False(modelo.EhNovo);
        Assert.Same(dto, modelo.Cupom);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_Edicao_Entao_DeveAtualizarERedirecionarSemConferirCodigo()
    {
        var dto = DtoValido();
        dto.CupomId = Guid.NewGuid();

        var resultado = await _controller.Edicao(dto);

        _cupomServiceMock.Verify(s => s.Atualizar(dto), Times.Once);
        _cupomServiceMock.Verify(s => s.CodigoJaExiste(It.IsAny<string>()), Times.Never);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_Edicao_Entao_DeveVoltarAoFormulario()
    {
        _controller.ModelState.AddModelError("Cupom.DataFim", "Informe a data de fim.");

        var resultado = await _controller.Edicao(DtoValido());

        Assert.IsType<ViewResult>(resultado);
        _cupomServiceMock.Verify(s => s.Atualizar(It.IsAny<CupomDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_Id_Quando_Ativar_Entao_DeveAtivarERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Ativar(id);

        _cupomServiceMock.Verify(s => s.Ativar(id), Times.Once);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_Id_Quando_Desativar_Entao_DeveDesativarERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Desativar(id);

        _cupomServiceMock.Verify(s => s.Desativar(id), Times.Once);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public void Dado_AcoesPost_Quando_InspecionarAtributos_Entao_DevemExigirAntiforgery()
    {
        var metodosPost = typeof(AdminCupomController).GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpPostAttribute), false).Length > 0)
            .ToList();

        Assert.Equal(4, metodosPost.Count);
        Assert.All(metodosPost, m =>
            Assert.NotEmpty(m.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false)));
    }
}
