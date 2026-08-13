using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class UsuarioServiceCadastroTests
{
    private readonly Mock<UserManager<ContaDeAcesso>> _userManagerMock;
    private readonly Mock<SignInManager<ContaDeAcesso>> _signInManagerMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceCadastroTests()
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
        var emailServiceMock = new Mock<IEmailService>();
        var loggerMock = new Mock<ILogger<UsuarioService>>();

        _usuarioService = new UsuarioService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object,
            emailServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_CadastrarUsuario_Entao_DeveCriarAsDuasMetadesComOMesmoId()
    {
        var dto = CriarCadastroDTO();
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .Callback<ContaDeAcesso, string>((conta, _) =>
                typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid()))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _usuarioService.CadastrarUsuario(dto);

        Assert.NotNull(resultado);
        Assert.Equal(dto.Nome, resultado.Nome);
        Assert.Equal(dto.Email, resultado.Email);
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.Is<Usuario>(u => u.Nome == dto.Nome)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_EmailJaCadastrado_Quando_CadastrarUsuario_Entao_DeveLancarInvalidOperationExceptionSemTocarNoUsuario()
    {
        var dto = CriarCadastroDTO();
        var erro = new IdentityError { Code = "DuplicateEmail", Description = "Email duplicado" };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .ReturnsAsync(IdentityResult.Failed(erro));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto));

        Assert.Equal("Os dados informados já estão associados a uma conta existente.", ex.Message);
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Dado_FalhaGeralDoIdentity_Quando_CadastrarUsuario_Entao_DeveLancarInvalidOperationException()
    {
        var dto = CriarCadastroDTO();
        var erro = new IdentityError { Code = "GenericError", Description = "Erro genérico no cadastro" };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .ReturnsAsync(IdentityResult.Failed(erro));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto));

        Assert.Contains("Erro genérico no cadastro", ex.Message);
    }

    [Fact]
    public async Task Dado_CpfJaCadastrado_Quando_CadastrarUsuario_Entao_DeveApagarAContaCriada()
    {
        // O e-mail passa pela checagem do Identity — a conta é criada — e só
        // então a gravação do Usuario falha porque o CPF já existe (índice
        // único, verificado na integração; aqui simulamos a falha vindo do
        // repositório). Sem compensação, sobraria uma conta com senha e sem
        // cadastro. RN-08 e CA-04 da spec 004.
        var dto = CriarCadastroDTO();
        ContaDeAcesso? contaCriada = null;
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .Callback<ContaDeAcesso, string>((conta, _) =>
            {
                typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());
                contaCriada = conta;
            })
            .ReturnsAsync(IdentityResult.Success);
        _usuarioRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Usuario>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SalvarAlteracoes(default))
            .ThrowsAsync(new InvalidOperationException("simulação: CPF duplicado"));
        _userManagerMock.Setup(u => u.DeleteAsync(It.IsAny<ContaDeAcesso>()))
            .ReturnsAsync(IdentityResult.Success);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto));

        _userManagerMock.Verify(
            u => u.DeleteAsync(It.Is<ContaDeAcesso>(c => c == contaCriada)),
            Times.Once);
    }

    [Fact]
    public async Task Dado_PapelInformado_Quando_CadastrarUsuario_Entao_DeveAtribuirOPapel()
    {
        var dto = CriarCadastroDTO();
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .Callback<ContaDeAcesso, string>((conta, _) =>
                typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ContaDeAcesso>(), "Administrador"))
            .ReturnsAsync(IdentityResult.Success);

        await _usuarioService.CadastrarUsuario(dto, "Administrador");

        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ContaDeAcesso>(), "Administrador"), Times.Once);
    }

    [Fact]
    public async Task Dado_FalhaAoAtribuirPapel_Quando_CadastrarUsuario_Entao_DeveApagarAContaCriada()
    {
        // Sem compensação aqui, sobraria uma conta de cliente que ninguém
        // pediu — a gestão de administradores não entrega promoção de conta
        // existente (fora de escopo da spec 005). RN-05 da spec 005.
        var dto = CriarCadastroDTO();
        ContaDeAcesso? contaCriada = null;
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ContaDeAcesso>(), dto.Senha!))
            .Callback<ContaDeAcesso, string>((conta, _) =>
            {
                typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());
                contaCriada = conta;
            })
            .ReturnsAsync(IdentityResult.Success);
        var erroPapel = new IdentityError { Description = "Papel inexistente" };
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ContaDeAcesso>(), "Administrador"))
            .ReturnsAsync(IdentityResult.Failed(erroPapel));
        _userManagerMock.Setup(u => u.DeleteAsync(It.IsAny<ContaDeAcesso>()))
            .ReturnsAsync(IdentityResult.Success);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto, "Administrador"));

        _userManagerMock.Verify(
            u => u.DeleteAsync(It.Is<ContaDeAcesso>(c => c == contaCriada)),
            Times.Once);
    }

    private static CadastroDTO CriarCadastroDTO() =>
        new()
        {
            Nome = "Cliente Teste",
            Email = "teste@exemplo.com",
            Celular = "11999999999",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "548.394.270-11",
            Senha = "SenhaForte123!"
        };
}
