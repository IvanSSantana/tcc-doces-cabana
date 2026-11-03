using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class CatalogoControllerTests
{
    private readonly Mock<ICatalogoService> _catalogoServiceMock;
    private readonly CatalogoController _controller;

    public CatalogoControllerTests()
    {
        _catalogoServiceMock = new Mock<ICatalogoService>();
        _controller = new CatalogoController(_catalogoServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        _catalogoServiceMock
            .Setup(s => s.Montar(It.IsAny<CriteriosDoCatalogoDTO>(), It.IsAny<int>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new CatalogoDTO());
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_Index_Entao_DeveRetornarView()
    {
        var resultado = await _controller.Index();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Index_Entao_DeveRetornarPartialView()
    {
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        var resultado = await _controller.Index();

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ResultadoCatalogo", partial.ViewName);
    }

    [Fact]
    public async Task Dado_ParametrosOmitidos_Quando_Index_Entao_DeveUsarOrdenacaoMelhorAvaliadosEPaginaUm()
    {
        await _controller.Index();

        _catalogoServiceMock.Verify(s => s.Montar(
            It.Is<CriteriosDoCatalogoDTO>(c => c.ApelidoDaCategoria == null && c.Ordenacao == OrdenacaoCatalogo.MelhorAvaliados),
            1,
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Dado_OrdenacaoMaisVendidos_Quando_Index_Entao_DeveExecutarSemSanear()
    {
        await _controller.Index(ordenacao: OrdenacaoCatalogo.MaisVendidos);

        _catalogoServiceMock.Verify(s => s.Montar(
            It.Is<CriteriosDoCatalogoDTO>(c => c.Ordenacao == OrdenacaoCatalogo.MaisVendidos),
            It.IsAny<int>(),
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidoDeCategoria_Quando_Index_Entao_DevePassarAoServico()
    {
        await _controller.Index(apelido: "doces");

        _catalogoServiceMock.Verify(s => s.Montar(
            It.Is<CriteriosDoCatalogoDTO>(c => c.ApelidoDaCategoria == "doces"),
            It.IsAny<int>(),
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Dado_TermoNaQuery_Quando_Index_Entao_DeveChegarAoServicoSemDeformacao()
    {
        await _controller.Index(termo: "Café");

        _catalogoServiceMock.Verify(s => s.Montar(
            It.Is<CriteriosDoCatalogoDTO>(c => c.Termo == "Café"),
            It.IsAny<int>(),
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidosDeSubcategoria_Quando_Index_Entao_DevemChegarAoServicoSemDeformacao()
    {
        await _controller.Index(apelido: "doces", subcategorias: ["barras", "potes"]);

        _catalogoServiceMock.Verify(s => s.Montar(
            It.Is<CriteriosDoCatalogoDTO>(c => c.ApelidosDeSubcategoria.Count == 2
                && c.ApelidosDeSubcategoria.Contains("barras")
                && c.ApelidosDeSubcategoria.Contains("potes")),
            It.IsAny<int>(),
            It.IsAny<Guid?>()), Times.Once);
    }
}
