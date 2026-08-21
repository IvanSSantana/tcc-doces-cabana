using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Mensagens;
using DocesCabana.Infrastructure.Identity.Services;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Controllers;

public class AutenticacaoControllerTests
{
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly AutenticacaoController _controller;

    public AutenticacaoControllerTests()
    {
        _usuarioServiceMock = new Mock<IUsuarioService>();

        // IUrlHelper.IsLocalUrl é membro real da interface (não extensão) —
        // um mock sem Setup devolve false sempre, então precisa da mesma
        // regra que a implementação real usa: caminho começando em "/",
        // sem ser "//" nem "/\" (protocolo-relativo), é local.
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>()))
            .Returns((string url) => !string.IsNullOrEmpty(url)
                && url[0] == '/'
                && !(url.Length > 1 && (url[1] == '/' || url[1] == '\\')));

        _controller = new AutenticacaoController(_usuarioServiceMock.Object)
        {
            Url = urlHelperMock.Object
        };
    }

    [Fact]
    public void Dado_RequisicaoValida_Quando_LoginGet_Entao_DeveRetornarView()
    {
        var resultado = _controller.Login();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_LoginPost_Entao_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Email", "Email é obrigatório");
        var dto = new LoginDTO();

        var resultado = await _controller.Login(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task Dado_CredenciaisValidas_Quando_LoginPost_Entao_DeveRedirecionarParaHome()
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
    public async Task Dado_ContaBloqueada_Quando_LoginPost_Entao_DeveAdicionarErroBloqueioERetornarView()
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
    public async Task Dado_CredenciaisInvalidas_Quando_LoginPost_Entao_DeveAdicionarErroERetornarView()
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
    public async Task Dado_RetornoLocal_Quando_LoginPost_Entao_DeveVoltarParaEle()
    {
        // RF-13 (spec 015): quem entra a partir de uma página do catálogo
        // volta para ela, não para a home.
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!" };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var resultado = await _controller.Login(dto, returnUrl: "/Catalogo/doces");

        var localRedirect = Assert.IsType<LocalRedirectResult>(resultado);
        Assert.Equal("/Catalogo/doces", localRedirect.Url);
    }

    [Fact]
    public async Task Dado_RetornoExterno_Quando_LoginPost_Entao_DeveIrParaHome()
    {
        // RF-14/RN-04: endereço de retorno para fora do site é descartado —
        // sem isso, a tela de login vira trampolim para outro domínio.
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!" };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var resultado = await _controller.Login(dto, returnUrl: "https://site-malicioso.com/golpe");

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Dado_RetornoProtocoloRelativo_Quando_LoginPost_Entao_DeveIrParaHome()
    {
        // "//site-malicioso.com" não começa com "/" seguido de caractere que
        // não seja "/" — IsLocalUrl recusa esse formato de propósito.
        var dto = new LoginDTO { Login = "teste@email.com", Senha = "Senha123!" };
        _usuarioServiceMock.Setup(s => s.RealizarLogin(dto.Login, dto.Senha, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var resultado = await _controller.Login(dto, returnUrl: "//site-malicioso.com");

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public void Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveRetornarView()
    {
        var resultado = _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new CadastroDTO();

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task Dado_EmailJaUsado_Quando_CadastroPost_Entao_DeveAdicionarErroERetornarView()
    {
        var dto = new CadastroDTO { Email = "existente@email.com", CPF = "54839427011" };

        _usuarioServiceMock.Setup(s => s.ContaJaExiste(dto.Email!, dto.CPF!))
            .ReturnsAsync(true);

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        _usuarioServiceMock.Verify(s => s.CadastrarUsuario(It.IsAny<CadastroDTO>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CpfJaUsado_Quando_CadastroPost_Entao_DeveAdicionarErroERetornarView()
    {
        var dto = new CadastroDTO { Email = "novo@email.com", CPF = "54839427011" };

        _usuarioServiceMock.Setup(s => s.ContaJaExiste(dto.Email!, dto.CPF!))
            .ReturnsAsync(true);

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        _usuarioServiceMock.Verify(s => s.CadastrarUsuario(It.IsAny<CadastroDTO>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_CadastroPost_Entao_DeveCadastrarERedirecionarParaLogin()
    {
        var dto = new CadastroDTO { Email = "novo@email.com", CPF = "54839427011" };
        _usuarioServiceMock.Setup(s => s.ContaJaExiste(dto.Email!, dto.CPF!))
            .ReturnsAsync(false);
        _usuarioServiceMock.Setup(s => s.CadastrarUsuario(dto, null))
            .ReturnsAsync(new UsuarioDTO());

        var resultado = await _controller.Cadastro(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redirectResult.ActionName);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_LogoutPost_Entao_DeveRealizarLogoutERedirecionarParaLogin()
    {
        _usuarioServiceMock.Setup(s => s.RealizarLogout())
            .Returns(Task.CompletedTask);

        var resultado = await _controller.Logout();

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redirectResult.ActionName);
        _usuarioServiceMock.Verify(s => s.RealizarLogout(), Times.Once);
    }

    [Fact]
    public void Dado_RequisicaoValida_Quando_EsqueceuSenhaGet_Entao_DeveRetornarView()
    {
        var resultado = _controller.EsqueceuSenha();

        Assert.IsType<ViewResult>(resultado);
    }

    private const string MensagemConfirmacaoEsqueceuSenha =
        "Se existir uma conta com esse login, enviamos um e-mail com o link de redefinição.";

    [Fact]
    public async Task Dado_LoginExistente_Quando_EsqueceuSenha_Entao_DeveGravarConfirmacaoEmTempData()
    {
        var dto = new EsqueceuSenhaDTO { Login = "existente@email.com" };
        var usuario = new UsuarioDTO { Email = "existente@email.com" };

        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Login))
            .ReturnsAsync(usuario);
        _usuarioServiceMock.Setup(s => s.GerarTokenRedefinicaoSenha(usuario.Email))
            .ReturnsAsync("token-redefinicao");
        _usuarioServiceMock.Setup(s => s.SolicitarRedefinicaoSenha(usuario.Email, It.IsAny<string>()))
            .ReturnsAsync(true);

        ConfigurarUrlEHttpContextETempData();

        var resultado = await _controller.EsqueceuSenha(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("EsqueceuSenha", redirectResult.ActionName);
        Assert.Equal(MensagemConfirmacaoEsqueceuSenha, _controller.TempData["Confirmacao"]);
        _usuarioServiceMock.Verify(s => s.SolicitarRedefinicaoSenha(usuario.Email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Dado_LoginInexistente_Quando_EsqueceuSenha_Entao_DeveGravarMesmaConfirmacaoENaoEnviarEmail()
    {
        var dto = new EsqueceuSenhaDTO { Login = "inexistente@email.com" };
        _usuarioServiceMock.Setup(s => s.BuscarPorLogin(dto.Login))
            .ReturnsAsync((UsuarioDTO?)null);

        ConfigurarUrlEHttpContextETempData();

        var resultado = await _controller.EsqueceuSenha(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("EsqueceuSenha", redirectResult.ActionName);
        Assert.Equal(MensagemConfirmacaoEsqueceuSenha, _controller.TempData["Confirmacao"]);
        _usuarioServiceMock.Verify(
            s => s.SolicitarRedefinicaoSenha(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_LoginMalformado_Quando_EsqueceuSenha_Entao_DeveRetornarViewSemConsultarServico()
    {
        _controller.ModelState.AddModelError("Login", "O formato do login deve ser um e-mail ou um CPF válido.");
        var dto = new EsqueceuSenhaDTO { Login = "abc" };

        var resultado = await _controller.EsqueceuSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        _usuarioServiceMock.Verify(s => s.BuscarPorLogin(It.IsAny<string>()), Times.Never);
    }

    private void ConfigurarUrlEHttpContextETempData()
    {
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

        var tempDataProviderMock = new Mock<ITempDataProvider>();
        tempDataProviderMock.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());
        _controller.TempData = new TempDataDictionary(httpContext, tempDataProviderMock.Object);
    }

    [Fact]
    public void Dado_TokenEEmail_Quando_RedefinirSenhaGet_Entao_DeveRetornarViewComDtoPreenchido()
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
    public async Task Dado_ModelStateInvalido_Quando_RedefinirSenhaPost_Entao_DeveRetornarViewComDto()
    {
        _controller.ModelState.AddModelError("Senha", "Senha é obrigatória");
        var dto = new RedefinirSenhaDTO();

        var resultado = await _controller.RedefinirSenha(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
    }

    [Fact]
    public async Task Dado_TokenValido_Quando_RedefinirSenhaPost_Entao_DeveConfirmarRedefinicaoERedirecionarParaLogin()
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
    public async Task Dado_TokenInvalido_Quando_RedefinirSenhaPost_Entao_DeveAdicionarErroERetornarView()
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
