using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.MVC.Controllers;
using DocesCabana.MVC.Models;
using DocesCabana.MVC.ViewComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IProdutoService> _produtoServiceMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _produtoServiceMock = new Mock<IProdutoService>();
        _controller = new HomeController(_produtoServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    // RF-04/RF-09 (spec 019): a home passa a pedir só os destaques, com o
    // mesmo limite que a vitrine exibe — não mais a loja inteira.
    [Fact]
    public async Task Dado_ProdutosCadastrados_Quando_Index_Entao_DeveRetornarViewComProdutos()
    {
        var produtosEsperados = new List<ProdutoDTO>
        {
            new() { ProdutoId = Guid.NewGuid(), Nome = "Bolo Cenoura", Preco = 10.00m },
            new() { ProdutoId = Guid.NewGuid(), Nome = "Brigadeiro", Preco = 5.00m }
        };

        _produtoServiceMock
            .Setup(s => s.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, null))
            .ReturnsAsync(produtosEsperados);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var model = Assert.IsAssignableFrom<List<ProdutoDTO>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Dado_VisitanteSemAutenticacao_Quando_Index_Entao_DevePedirDestaquesSemUsuario()
    {
        // CA-12
        _produtoServiceMock
            .Setup(s => s.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, null))
            .ReturnsAsync([]);

        await _controller.Index();

        _produtoServiceMock.Verify(
            s => s.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, null), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Index_Entao_DevePedirDestaquesComOIdDoUsuario()
    {
        // CA-11
        var usuarioId = Guid.NewGuid();
        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())], "TesteAutenticacao");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identidade);

        _produtoServiceMock
            .Setup(s => s.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, usuarioId))
            .ReturnsAsync([]);

        await _controller.Index();

        _produtoServiceMock.Verify(
            s => s.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, usuarioId), Times.Once);
    }

    [Fact]
    public void Dado_ErroNaRequisicao_Quando_Error_Entao_DeveRetornarViewComErrorViewModel()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var resultado = _controller.Error();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal("test-trace-id", model.RequestId);
    }
}
