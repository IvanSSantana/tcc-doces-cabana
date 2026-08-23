using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class CarrinhoControllerTests
{
    private readonly Mock<ICarrinhoService> _carrinhoServiceMock;
    private readonly CarrinhoController _controller;

    public CarrinhoControllerTests()
    {
        _carrinhoServiceMock = new Mock<ICarrinhoService>();
        var httpContext = new DefaultHttpContext { Session = new SessaoFalsa() };
        _controller = new CarrinhoController(_carrinhoServiceMock.Object)
        {
            // Visitante anônimo por padrão; ConfigurarUsuarioAutenticado
            // substitui isto nos testes que precisam de um usuário logado.
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Index_Entao_DeveUsarOServicoEDevolverView()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var carrinho = new CarrinhoDTO();
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(carrinho);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(carrinho, viewResult.Model);
        _carrinhoServiceMock.Verify(s => s.ObterDoUsuario(usuarioId), Times.Once);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Index_Entao_DeveDevolverPartialView()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Index();

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ItensDoCarrinho", partial.ViewName);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Acrescentar_Entao_DeveChamarOServicoERedirecionar()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 3);

        _carrinhoServiceMock.Verify(s => s.Acrescentar(usuarioId, produtoId, 3), Times.Once);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_QuantidadeOmitida_Quando_Acrescentar_Entao_DeveUsarUma()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.Acrescentar(produtoId);

        _carrinhoServiceMock.Verify(s => s.Acrescentar(usuarioId, produtoId, 1), Times.Once);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Acrescentar_Entao_DeveDevolverPartialView()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 1);

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ItensDoCarrinho", partial.ViewName);
    }

    // Fase 6 (spec 017): o visitante também acrescenta — não há mais
    // desafio de login nas ações de escrita. O carrinho dele vive na
    // sessão, não no banco (CA-12).

    [Fact]
    public async Task Dado_Visitante_Quando_Acrescentar_Entao_DeveGuardarNaSessaoENaoUsarOBanco()
    {
        var produtoId = Guid.NewGuid();
        _carrinhoServiceMock
            .Setup(s => s.AcrescentarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>(), produtoId, (short)2))
            .ReturnsAsync([new ItemDoCarrinhoDTO(produtoId, 2)]);
        _carrinhoServiceMock.Setup(s => s.MontarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>())).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 2);

        _carrinhoServiceMock.Verify(s => s.AcrescentarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>(), produtoId, (short)2), Times.Once);
        _carrinhoServiceMock.Verify(s => s.Acrescentar(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_Visitante_Quando_Index_Entao_DeveMontarDaSessao()
    {
        var carrinho = new CarrinhoDTO();
        _carrinhoServiceMock.Setup(s => s.MontarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>())).ReturnsAsync(carrinho);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(carrinho, viewResult.Model);
        _carrinhoServiceMock.Verify(s => s.ObterDoUsuario(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_AlterarQuantidade_Entao_DeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.AlterarQuantidade(produtoId, 5);

        _carrinhoServiceMock.Verify(s => s.AlterarQuantidade(usuarioId, produtoId, 5), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Remover_Entao_DeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.Remover(produtoId);

        _carrinhoServiceMock.Verify(s => s.Remover(usuarioId, produtoId), Times.Once);
    }

    private void ConfigurarUsuarioAutenticado(Guid usuarioId)
    {
        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())], "TesteAutenticacao");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identidade),
            Session = _controller.ControllerContext.HttpContext.Session,
        };

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    // ISession real precisa de um IDistributedCache configurado — pesado
    // demais para um teste de unidade que só quer provar que o controlador
    // lê e escreve na sessão certa. Um dicionário em memória basta.
    private sealed class SessaoFalsa : ISession
    {
        private readonly Dictionary<string, byte[]> _valores = new();

        public bool IsAvailable => true;
        public string Id => "sessao-de-teste";
        public IEnumerable<string> Keys => _valores.Keys;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
        public void Set(string key, byte[] value) => _valores[key] = value;
        public void Remove(string key) => _valores.Remove(key);
        public void Clear() => _valores.Clear();
    }
}
