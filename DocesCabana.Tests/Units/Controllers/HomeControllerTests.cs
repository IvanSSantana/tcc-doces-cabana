using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.MVC.Controllers;
using DocesCabana.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
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
        _controller = new HomeController(_produtoServiceMock.Object);
    }

    [Fact]
    public async Task Dado_ProdutosCadastrados_Quando_Index_Entao_DeveRetornarViewComProdutos()
    {
        var produtosEsperados = new List<ProdutoDTO>
        {
            new() { ProdutoId = Guid.NewGuid(), Nome = "Bolo Cenoura", Preco = 10.00m },
            new() { ProdutoId = Guid.NewGuid(), Nome = "Brigadeiro", Preco = 5.00m }
        };

        _produtoServiceMock.Setup(s => s.BuscarTodosProdutos())
            .ReturnsAsync(produtosEsperados);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var model = Assert.IsAssignableFrom<List<ProdutoDTO>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        _produtoServiceMock.Verify(s => s.BuscarTodosProdutos(), Times.Once);
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
