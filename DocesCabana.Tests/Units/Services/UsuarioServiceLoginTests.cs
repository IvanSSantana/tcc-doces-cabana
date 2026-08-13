using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class UsuarioServiceLoginTests
{
    private readonly Mock<UserManager<ContaDeAcesso>> _userManagerMock;
    private readonly Mock<SignInManager<ContaDeAcesso>> _signInManagerMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceLoginTests()
    {
        var storeMock = new Mock<IUserStore<ContaDeAcesso>>();
        _userManagerMock = new Mock<UserManager<ContaDeAcesso>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ContaDeAcesso>>();
        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        var signInLoggerMock = new Mock<ILogger<SignInManager<ContaDeAcesso>>>();
        var schemesMock = new Mock<IAuthenticationSchemeProvider>();
        var confirmationMock = new Mock<IUserConfirmation<ContaDeAcesso>>();

        _signInManagerMock = new Mock<SignInManager<ContaDeAcesso>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            userClaimsPrincipalFactoryMock.Object,
            optionsMock.Object,
            signInLoggerMock.Object,
            schemesMock.Object,
            confirmationMock.Object);

        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        var emailServiceMock = new Mock<IEmailService>();
        var loggerMock = new Mock<ILogger<UsuarioService>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        _usuarioService = new UsuarioService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _usuarioRepositoryMock.Object,
            unitOfWorkMock.Object,
            emailServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Dado_LoginPorCpfSemPontuacao_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var cpf = "54839427011";
        var (usuario, conta) = CriarParComCpf(cpf, "cliente@teste.com");

        _userManagerMock.Setup(u => u.FindByEmailAsync(cpf))
            .ReturnsAsync((ContaDeAcesso?)null);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorCpf(cpf))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.UsuarioId.ToString()))
            .ReturnsAsync(conta);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(conta.Email!, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        var resultado = await _usuarioService.RealizarLogin(cpf, "senha123", false);

        Assert.True(resultado.Succeeded);
    }

    [Fact]
    public async Task Dado_LoginPorCpfPontuado_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var cpfPontuado = "548.394.270-11";
        var cpfLimpo = "54839427011";
        var (usuario, conta) = CriarParComCpf(cpfLimpo, "cliente@teste.com");

        _userManagerMock.Setup(u => u.FindByEmailAsync(cpfPontuado))
            .ReturnsAsync((ContaDeAcesso?)null);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorCpf(cpfLimpo))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.UsuarioId.ToString()))
            .ReturnsAsync(conta);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(conta.Email!, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        var resultado = await _usuarioService.RealizarLogin(cpfPontuado, "senha123", false);

        Assert.True(resultado.Succeeded);
    }

    [Fact]
    public async Task Dado_LoginPorEmail_Quando_RealizarLogin_Entao_DeveAutenticar()
    {
        var email = "cliente@teste.com";
        var (usuario, conta) = CriarPar(email);

        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta.Id))
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
            .ReturnsAsync((ContaDeAcesso?)null);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorCpf(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

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
        var (usuario, conta) = CriarPar(email);

        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta.Id))
            .ReturnsAsync(usuario);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(email, "senha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        await _usuarioService.RealizarLogin(email, "senha123", false);

        _signInManagerMock.Verify(
            s => s.PasswordSignInAsync(email, "senha123", false, true),
            Times.Once);
    }

    private static (Usuario Usuario, ContaDeAcesso Conta) CriarPar(string email, string cpf = "54839427011")
    {
        var conta = new ContaDeAcesso(email);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());

        var usuario = new Usuario(conta.Id, "Cliente Teste", cpf, "11999999999", new DateTime(1990, 1, 1));

        return (usuario, conta);
    }

    private static (Usuario Usuario, ContaDeAcesso Conta) CriarParComCpf(string cpf, string email) =>
        CriarPar(email, cpf);
}
