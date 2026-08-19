using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.MVC.Controllers;
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
        _controller = new CatalogoController(_catalogoServiceMock.Object);

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
    public async Task Dado_ParametrosOmitidos_Quando_Index_Entao_DeveUsarOrdenacaoNomeAZEPaginaUm()
    {
        await _controller.Index();

        _catalogoServiceMock.Verify(s => s.Montar(
            null,
            It.Is<FiltroCatalogoDTO>(f => f.Ordenacao == OrdenacaoCatalogo.NomeAZ),
            1), Times.Once);
    }

    [Fact]
    public async Task Dado_OrdenacaoMaisVendidos_Quando_Index_Entao_DeveSanearParaNomeAZ()
    {
        // RN-07: "Mais vendidos" é anunciada, não oferecida — mesmo que
        // alguém force o valor pela URL, o controller recusa.
        await _controller.Index(ordenacao: OrdenacaoCatalogo.MaisVendidos);

        _catalogoServiceMock.Verify(s => s.Montar(
            It.IsAny<string?>(),
            It.Is<FiltroCatalogoDTO>(f => f.Ordenacao == OrdenacaoCatalogo.NomeAZ),
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
