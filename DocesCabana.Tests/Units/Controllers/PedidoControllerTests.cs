using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class PedidoControllerTests
{
    private readonly Mock<IPedidoService> _pedidoServiceMock;
    private readonly Mock<ICarrinhoService> _carrinhoServiceMock;
    private readonly PedidoController _controller;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public PedidoControllerTests()
    {
        _pedidoServiceMock = new Mock<IPedidoService>();
        _carrinhoServiceMock = new Mock<ICarrinhoService>();

        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())], "TesteAutenticacao");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidade) };

        _controller = new PedidoController(_pedidoServiceMock.Object, _carrinhoServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        // Padrão para os testes que chegam a reexibir a tela do carrinho —
        // sobrescrito nos testes que precisam de valores específicos.
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(new CarrinhoDTO());
        _pedidoServiceMock
            .Setup(s => s.MontarPasso(It.IsAny<PassoDoFechamento>(), It.IsAny<CarrinhoDTO>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>()))
            .ReturnsAsync(new PassoDoFechamentoDTO());
    }

    private static FechamentoDePedidoDTO CriarDados() => new()
    {
        EnderecoId = Guid.NewGuid(),
        ServicoDeEntregaId = 1,
        MetodoPagamento = MetodoPagamento.Pix,
        ValorDosProdutosExibido = 10m,
        ValorDoFreteExibido = 8m
    };

    [Fact]
    public async Task Dado_FechamentoComSucesso_Quando_Fechar_Entao_DeveRedirecionarParaConfirmacao()
    {
        var pedidoId = Guid.NewGuid();
        _pedidoServiceMock
            .Setup(s => s.Fechar(_usuarioId, It.IsAny<FechamentoDePedidoDTO>()))
            .ReturnsAsync(ResultadoDoFechamentoDTO.ParaSucesso(pedidoId));

        var resultado = await _controller.Fechar(CriarDados());

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(PedidoController.Confirmacao), redirecionamento.ActionName);
        Assert.Equal(pedidoId, redirecionamento.RouteValues!["id"]);
    }

    [Fact]
    public async Task Dado_FechamentoRecusadoPeloServico_Quando_Fechar_Entao_DeveDevolverAViewComModelStateInvalido()
    {
        _pedidoServiceMock
            .Setup(s => s.Fechar(_usuarioId, It.IsAny<FechamentoDePedidoDTO>()))
            .ReturnsAsync(ResultadoDoFechamentoDTO.ParaRecusa("Seu carrinho está vazio."));

        var resultado = await _controller.Fechar(CriarDados());

        Assert.IsType<ViewResult>(resultado);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Dado_ModelStateJaInvalido_Quando_Fechar_Entao_NuncaDeveChamarOServicoDeFechamento()
    {
        _controller.ModelState.AddModelError("EnderecoId", "Escolha um endereço de entrega!");

        var resultado = await _controller.Fechar(CriarDados());

        Assert.IsType<ViewResult>(resultado);
        _pedidoServiceMock.Verify(s => s.Fechar(It.IsAny<Guid>(), It.IsAny<FechamentoDePedidoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PedidoProprio_Quando_Confirmacao_Entao_DeveDevolverAViewComOResultado()
    {
        var pedidoId = Guid.NewGuid();
        var confirmacao = new ConfirmacaoDePedidoDTO { PedidoId = pedidoId, Numero = "ABCD1234" };
        _pedidoServiceMock.Setup(s => s.ObterConfirmacao(pedidoId, _usuarioId)).ReturnsAsync(confirmacao);

        var resultado = await _controller.Confirmacao(pedidoId);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(confirmacao, viewResult.Model);
    }

    [Fact]
    public async Task Dado_PedidoAlheioOuInexistente_Quando_Confirmacao_Entao_DeveDevolverNotFound()
    {
        var pedidoId = Guid.NewGuid();
        _pedidoServiceMock.Setup(s => s.ObterConfirmacao(pedidoId, _usuarioId)).ReturnsAsync((ConfirmacaoDePedidoDTO?)null);

        var resultado = await _controller.Confirmacao(pedidoId);

        Assert.IsType<NotFoundResult>(resultado);
    }
}
