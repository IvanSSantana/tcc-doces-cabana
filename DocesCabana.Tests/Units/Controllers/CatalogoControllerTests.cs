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
            // spec 014: o Index passou a ler Request.Headers para decidir
            // entre página inteira e partial — precisa de um HttpContext de
            // verdade, não do padrão nulo do Controller fora do pipeline MVC.
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        _catalogoServiceMock
            .Setup(s => s.Montar(It.IsAny<string?>(), It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>()))
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
        // RF-01 (spec 014): um endereço, duas representações — quem pede via
        // catalogo.js recebe só o bloco do resultado, não a página inteira.
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        var resultado = await _controller.Index();

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ResultadoCatalogo", partial.ViewName);
    }

    [Fact]
    public async Task Dado_ParametrosOmitidos_Quando_Index_Entao_DeveUsarOrdenacaoMelhorAvaliadosEPaginaUm()
    {
        // RF-16 (spec 014): o padrão deixou de ser "Nome (A-Z)" — a base de
        // demonstração passou a ter avaliação suficiente para sustentar isso.
        await _controller.Index();

        _catalogoServiceMock.Verify(s => s.Montar(
            null,
            It.Is<FiltroCatalogoDTO>(f => f.Ordenacao == OrdenacaoCatalogo.MelhorAvaliados),
            1), Times.Once);
    }

    [Fact]
    public async Task Dado_OrdenacaoMaisVendidos_Quando_Index_Entao_DeveSanearParaMelhorAvaliados()
    {
        // RN-07: "Mais vendidos" é anunciada, não oferecida — mesmo que
        // alguém force o valor pela URL, o controller recusa e cai no
        // padrão atual (spec 014, RF-16).
        await _controller.Index(ordenacao: OrdenacaoCatalogo.MaisVendidos);

        _catalogoServiceMock.Verify(s => s.Montar(
            It.IsAny<string?>(),
            It.Is<FiltroCatalogoDTO>(f => f.Ordenacao == OrdenacaoCatalogo.MelhorAvaliados),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidoDeCategoria_Quando_Index_Entao_DevePassarAoServico()
    {
        await _controller.Index(apelido: "doces");

        _catalogoServiceMock.Verify(s => s.Montar(
            "doces", It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>()), Times.Once);
    }
}
