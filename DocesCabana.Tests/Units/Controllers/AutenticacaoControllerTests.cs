using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity.Services;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Controllers;

public class AutenticacaoControllerTests
{
    private readonly Mock<ILogger<AutenticacaoController>> _loggerMock;
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly AutenticacaoController _controller;

    public AutenticacaoControllerTests()
    {
        _loggerMock = new Mock<ILogger<AutenticacaoController>>();
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _controller = new AutenticacaoController(_loggerMock.Object, _usuarioServiceMock.Object);
    }

    [Fact]
    public void Login_Get_DeveRetornarView()
    {
        var resultado = _controller.Login();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Login_Post_ModelStateInvalido_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Email", "Email é obrigatório");
        var dto = new LoginDTO();

        var resultado = await _controller.Login(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task Login_Post_Sucesso_DeveRedirecionarParaHome()
    {
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!", LembrarMe = true };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var resultado = await _controller.Login(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Login_Post_Bloqueado_DeveAdicionarErroBloqueioERetornarView()
    {
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!" };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var resultado = await _controller.Login(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("Conta bloqueada", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Login_Post_FalhaCredenciais_DeveAdicionarErroCredenciaisERetornarView()
    {
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!" };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var resultado = await _controller.Login(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("E-mail ou senha incorreto", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Cadastro_Get_DeveRetornarView()
    {
        var resultado = _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Cadastro_Post_ModelStateInvalido_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new CadastroDTO();

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task Cadastro_Post_UsuarioExistente_DeveAdicionarErroERetornarView()
    {
        var dto = new CadastroDTO { Email = "existente@email.com", CPF = "54839427011" };
        var usuarioExistente = new UsuarioDTO { Email = "existente@email.com" };

        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Email))
            .ReturnsAsync(usuarioExistente);

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("Os dados informados já estão associados", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Cadastro_Post_Sucesso_DeveCadastrarERedirecionarParaLogin()
    {
        var dto = new CadastroDTO { Email = "novo@email.com", CPF = "54839427011" };
        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Email))
            .ReturnsAsync((UsuarioDTO?)null);
        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.CPF))
            .ReturnsAsync((UsuarioDTO?)null);
        _usuarioServiceMock.Setup(s => s.CadastrarUsuario(dto))
            .ReturnsAsync(new UsuarioDTO());

        var resultado = await _controller.Cadastro(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redirectResult.ActionName);
    }

    [Fact]
    public async Task Logout_Post_DeveRealizarLogoutERedirecionarParaLogin()
    {
        _usuarioServiceMock.Setup(s => s.RealizarLogout())
            .Returns(Task.CompletedTask);

        var resultado = await _controller.Logout();

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redirectResult.ActionName);
        _usuarioServiceMock.Verify(s => s.RealizarLogout(), Times.Once);
    }

    [Fact]
    public void EsqueceuSenha_Get_DeveRetornarView()
    {
        var resultado = _controller.EsqueceuSenha();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task EsqueceuSenha_Post_UsuarioInexistente_DeveAdicionarErroMensagemERetornarView()
    {
        var dto = new EsqueceuSenhaDTO { Login = "inexistente@email.com" };
        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Login))
            .ReturnsAsync((UsuarioDTO?)null);

        var resultado = await _controller.EsqueceuSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("Foi enviado um e-mail de confirmação", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task EsqueceuSenha_Post_UsuarioExistente_DeveGerarTokenEnviarEmailERetornarView()
    {
        var dto = new EsqueceuSenhaDTO { Login = "existente@email.com" };
        var usuario = new UsuarioDTO { Email = "existente@email.com" };

        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Login))
            .ReturnsAsync(usuario);
        _usuarioServiceMock.Setup(s => s.GerarTokenRedefinicaoSenha(usuario.Email))
            .ReturnsAsync("token-redefinicao");
        _usuarioServiceMock.Setup(s => s.SolicitarRedefinicaoSenha(usuario.Email, It.IsAny<string>()))
            .ReturnsAsync(true);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/redefinir-senha");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");

        _controller.Url = urlHelperMock.Object;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var resultado = await _controller.EsqueceuSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("Foi enviado um e-mail de confirmação", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        _usuarioServiceMock.Verify(s => s.SolicitarRedefinicaoSenha(usuario.Email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void RedefinirSenha_Get_DeveRetornarViewComDtoPreenchido()
    {
        var token = "token-teste";
        var email = "teste@email.com";

        var resultado = _controller.RedefinirSenha(token, email);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var model = Assert.IsType<RedefinirSenhaDTO>(viewResult.Model);
        Assert.Equal(token, model.Token);
        Assert.Equal(email, model.Email);
    }

    [Fact]
    public async Task RedefinirSenha_Post_ModelStateInvalido_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Senha", "Senha é obrigatória");
        var dto = new RedefinirSenhaDTO();

        var resultado = await _controller.RedefinirSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task RedefinirSenha_Post_Sucesso_DeveConfirmarRedefinicaoERedirecionarParaLogin()
    {
        var dto = new RedefinirSenhaDTO { Email = "teste@email.com", Token = "token-teste", Senha = "NovaSenha123!" };
        _usuarioServiceMock.Setup(s => s.ConfirmarRedefinicaoSenha(dto.Email, dto.Token, dto.Senha))
            .ReturnsAsync(true);

        var resultado = await _controller.RedefinirSenha(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Autenticacao", redirectResult.ControllerName);
    }

    [Fact]
    public async Task RedefinirSenha_Post_Falha_DeveAdicionarErroERetornarView()
    {
        var dto = new RedefinirSenhaDTO { Email = "teste@email.com", Token = "token-teste", Senha = "NovaSenha123!" };
        _usuarioServiceMock.Setup(s => s.ConfirmarRedefinicaoSenha(dto.Email, dto.Token, dto.Senha))
            .ReturnsAsync(false);

        var resultado = await _controller.RedefinirSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains("Erro ao redefinir senha", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }
}
