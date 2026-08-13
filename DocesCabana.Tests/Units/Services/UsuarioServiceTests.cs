using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class UsuarioServiceTests
{
    private readonly Mock<UserManager<ContaDeAcesso>> _userManagerMock;
    private readonly Mock<SignInManager<ContaDeAcesso>> _signInManagerMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<UsuarioService>> _loggerMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceTests()
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
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<UsuarioService>>();

        _usuarioService = new UsuarioService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Dado_IdExistente_Quando_BuscarUsuarioPorId_Entao_DeveRetornarUsuarioDto()
    {
        var id = Guid.NewGuid();
        var (usuario, conta) = CriarParUsuarioConta(id);
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync(conta);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(id))
            .ReturnsAsync(usuario);

        var resultado = await _usuarioService.BuscarUsuarioPorId(id);

        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);
    }

    [Fact]
    public async Task Dado_IdInexistente_Quando_BuscarUsuarioPorId_Entao_DeveLancarKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync((ContaDeAcesso?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.BuscarUsuarioPorId(id));
    }

    [Fact]
    public async Task Dado_EmailExistente_Quando_BuscarPorLogin_Entao_DeveRetornarUsuarioDto()
    {
        var email = "teste@exemplo.com";
        var (usuario, conta) = CriarParUsuarioConta(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta.Id))
            .ReturnsAsync(usuario);

        var resultado = await _usuarioService.BuscarPorLogin(email);

        Assert.NotNull(resultado);
        Assert.Equal(email, resultado.Email);
    }

    [Fact]
    public async Task Dado_CpfExistente_Quando_BuscarPorLogin_Entao_DeveNormalizarCPFRetornarUsuarioDto()
    {
        var login = "548.394.270-11";
        var cpfLimpo = "54839427011";
        var (usuario, conta) = CriarParUsuarioConta(Guid.NewGuid(), cpf: cpfLimpo);

        _userManagerMock.Setup(u => u.FindByEmailAsync(login))
            .ReturnsAsync((ContaDeAcesso?)null);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorCpf(cpfLimpo))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.UsuarioId.ToString()))
            .ReturnsAsync(conta);

        var resultado = await _usuarioService.BuscarPorLogin(login);

        Assert.NotNull(resultado);
        Assert.Equal(cpfLimpo, resultado.CPF);
    }

    [Fact]
    public async Task Dado_LoginInexistente_Quando_BuscarPorLogin_Entao_DeveRetornarNull()
    {
        var login = "inexistente@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(login))
            .ReturnsAsync((ContaDeAcesso?)null);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorCpf(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _usuarioService.BuscarPorLogin(login);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_AlterarDadosUsuario_Entao_DeveRetornarUsuarioDtoAtualizado()
    {
        var id = Guid.NewGuid();
        var (usuario, conta) = CriarParUsuarioConta(id);
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync(conta);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(id))
            .ReturnsAsync(usuario);

        var dto = new UsuarioDTO
        {
            Id = id,
            Nome = "Novo Nome",
            Celular = "(11) 98888-8888",
            DataNascimento = new DateTime(1995, 5, 5),
            CPF = "54839427011"
        };

        var resultado = await _usuarioService.AlterarDadosUsuario(dto);

        Assert.NotNull(resultado);
        Assert.Equal("Novo Nome", resultado.Nome);
        Assert.Equal("11988888888", resultado.Celular);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_AlterarDadosUsuario_Entao_DeveLancarKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync((ContaDeAcesso?)null);

        var dto = new UsuarioDTO { Id = id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.AlterarDadosUsuario(dto));
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_RealizarLogout_Entao_DeveChamarSignOutAsync()
    {
        _signInManagerMock.Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        await _usuarioService.RealizarLogout();

        _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task Dado_EmailExistente_Quando_GerarTokenRedefinicaoSenha_Entao_DeveRetornarToken()
    {
        var email = "senha@email.com";
        var conta = CriarConta(email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(conta))
            .ReturnsAsync("token-valido");

        var token = await _usuarioService.GerarTokenRedefinicaoSenha(email);

        Assert.Equal("token-valido", token);
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_GerarTokenRedefinicaoSenha_Entao_DeveLancarKeyNotFoundException()
    {
        var email = "senha@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((ContaDeAcesso?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.GerarTokenRedefinicaoSenha(email));
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_SolicitarRedefinicaoSenha_Entao_DeveRetornarFalse()
    {
        var email = "senha@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((ContaDeAcesso?)null);

        var resultado = await _usuarioService.SolicitarRedefinicaoSenha(email, "corpo");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_UsuarioExistente_Quando_SolicitarRedefinicaoSenha_Entao_DeveEnviarEmailERetornarTrue()
    {
        var email = "senha@email.com";
        var conta = CriarConta(email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _emailServiceMock.Setup(e => e.EnviarEmail(email, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var resultado = await _usuarioService.SolicitarRedefinicaoSenha(email, "corpo");

        Assert.True(resultado);
        _emailServiceMock.Verify(e => e.EnviarEmail(email, "Doces Cabana - Redefinição de Senha", "corpo"), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_ConfirmarRedefinicaoSenha_Entao_DeveRetornarFalse()
    {
        var email = "senha@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((ContaDeAcesso?)null);

        var resultado = await _usuarioService.ConfirmarRedefinicaoSenha(email, "token", "novaSenha");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_TokenValido_Quando_ConfirmarRedefinicaoSenha_Entao_DeveRetornarTrue()
    {
        var email = "senha@email.com";
        var conta = CriarConta(email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _userManagerMock.Setup(u => u.ResetPasswordAsync(conta, "token", "novaSenha"))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _usuarioService.ConfirmarRedefinicaoSenha(email, "token", "novaSenha");

        Assert.True(resultado);
    }

    [Fact]
    public async Task Dado_ParametrosInvalidos_Quando_ConfirmarEmailDoUsuario_Entao_DeveRetornarFalse()
    {
        var res1 = await _usuarioService.ConfirmarEmailDoUsuario("", "token");
        var res2 = await _usuarioService.ConfirmarEmailDoUsuario("email@email.com", "");

        Assert.False(res1);
        Assert.False(res2);
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_ConfirmarEmailDoUsuario_Entao_DeveRetornarFalse()
    {
        var email = "email@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((ContaDeAcesso?)null);

        var resultado = await _usuarioService.ConfirmarEmailDoUsuario(email, "token");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_TokenValido_Quando_ConfirmarEmailDoUsuario_Entao_DeveRetornarTrue()
    {
        var email = "email@email.com";
        var conta = CriarConta(email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(conta);
        _userManagerMock.Setup(u => u.ConfirmEmailAsync(conta, "token"))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _usuarioService.ConfirmarEmailDoUsuario(email, "token");

        Assert.True(resultado);
    }

    internal static ContaDeAcesso CriarConta(string email = "teste@exemplo.com")
    {
        var conta = new ContaDeAcesso(email);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());
        return conta;
    }

    internal static (Usuario Usuario, ContaDeAcesso Conta) CriarParUsuarioConta(
        Guid id, string email = "teste@exemplo.com", string cpf = "54839427011")
    {
        var conta = new ContaDeAcesso(email);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, id);

        var usuario = new Usuario(id, "Cliente Teste", cpf, "11999999999", new DateTime(1990, 1, 1));

        return (usuario, conta);
    }
}
