using DocesCabana.Application.Contracts.Services;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class UsuarioServiceLoginTests
{
    private readonly Mock<UserManager<Usuario>> _userManagerMock;
    private readonly Mock<SignInManager<Usuario>> _signInManagerMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceLoginTests()
    {
        var storeMock = new Mock<IUserStore<Usuario>>();
        _userManagerMock = new Mock<UserManager<Usuario>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<Usuario>>();
        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        var signInLoggerMock = new Mock<ILogger<SignInManager<Usuario>>>();
        var schemesMock = new Mock<IAuthenticationSchemeProvider>();
        var confirmationMock = new Mock<IUserConfirmation<Usuario>>();

        _signInManagerMock = new Mock<SignInManager<Usuario>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            userClaimsPrincipalFactoryMock.Object,
            optionsMock.Object,
            signInLoggerMock.Object,
            schemesMock.Object,
            confirmationMock.Object);

        var emailServiceMock = new Mock<IEmailService>();
        var loggerMock = new Mock<ILogger<UsuarioService>>();

        _usuarioService = new UsuarioService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            emailServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Dado_LoginPorCpfSemPontuacao_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var cpf = "54839427011";
        var usuario = CriarUsuario(email: "cliente@teste.com", cpf: cpf);

        _userManagerMock.Setup(u => u.FindByEmailAsync(cpf))
            .ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.Users)
            .Returns(new TestAsyncEnumerable<Usuario>(new List<Usuario> { usuario }));
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(usuario.Email!, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        var resultado = await _usuarioService.RealizarLogin(cpf, "senha123", false);

        Assert.True(resultado.Succeeded);
    }

    [Fact]
    public async Task Dado_LoginPorCpfPontuado_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var cpfPontuado = "548.394.270-11";
        var cpfLimpo = "54839427011";
        var usuario = CriarUsuario(email: "cliente@teste.com", cpf: cpfLimpo);

        _userManagerMock.Setup(u => u.FindByEmailAsync(cpfPontuado))
            .ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.Users)
            .Returns(new TestAsyncEnumerable<Usuario>(new List<Usuario> { usuario }));
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(usuario.Email!, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        var resultado = await _usuarioService.RealizarLogin(cpfPontuado, "senha123", false);

        Assert.True(resultado.Succeeded);
    }

    [Fact]
    public async Task Dado_LoginPorEmail_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var email = "cliente@teste.com";
        var usuario = CriarUsuario(email: email);

        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(email, "senha123", true, true))
            .ReturnsAsync(SignInResult.Success);

        var resultado = await _usuarioService.RealizarLogin(email, "senha123", true);

        Assert.True(resultado.Succeeded);
    }

    [Fact]
    public async Task Dado_LoginInexistente_Quando_RealizarLogin_Entao_DeveRetornarFailed()
    {
        var login = "inexistente@teste.com";

        _userManagerMock.Setup(u => u.FindByEmailAsync(login))
            .ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.Users)
            .Returns(new TestAsyncEnumerable<Usuario>(new List<Usuario>()));

        var resultado = await _usuarioService.RealizarLogin(login, "senha123", false);

        Assert.False(resultado.Succeeded);
        _signInManagerMock.Verify(
            s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_CredenciaisValidas_Quando_RealizarLogin_Entao_DeveHabilitarBloqueioPorTentativas()
    {
        var email = "cliente@teste.com";
        var usuario = CriarUsuario(email: email);

        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(email, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        await _usuarioService.RealizarLogin(email, "senha123", false);

        _signInManagerMock.Verify(
            s => s.PasswordSignInAsync(email, "senha123", false, true),
            Times.Once);
    }

    private static Usuario CriarUsuario(string email, string cpf = "54839427011")
    {
        var usuario = new Usuario("Cliente Teste", email, "11999999999", new DateTime(1990, 1, 1), cpf);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(usuario, Guid.NewGuid());
        return usuario;
    }
}
