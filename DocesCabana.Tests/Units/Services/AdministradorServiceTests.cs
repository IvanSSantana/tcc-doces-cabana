using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Constants;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class AdministradorServiceTests
{
    private readonly Mock<UserManager<ContaDeAcesso>> _userManagerMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly AdministradorService _administradorService;

    public AdministradorServiceTests()
    {
        var storeMock = new Mock<IUserStore<ContaDeAcesso>>();
        _userManagerMock = new Mock<UserManager<ContaDeAcesso>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _usuarioServiceMock = new Mock<IUsuarioService>();

        _administradorService = new AdministradorService(
            _userManagerMock.Object,
            _usuarioRepositoryMock.Object,
            _usuarioServiceMock.Object);
    }

    [Fact]
    public async Task Dado_DoisAdministradores_Quando_ListarAdministradores_Entao_DeveRetornarNomeEEmailDeCada()
    {
        var conta1 = CriarConta("admin1@doces.com");
        var conta2 = CriarConta("admin2@doces.com");
        var usuario1 = new Usuario(conta1.Id, "Admin Um", "52998224725", "11987654321", new DateTime(1990, 1, 1));
        var usuario2 = new Usuario(conta2.Id, "Admin Dois", "11144477735", "11987654322", new DateTime(1991, 2, 2));

        _userManagerMock.Setup(u => u.GetUsersInRoleAsync("Administrador"))
            .ReturnsAsync(new List<ContaDeAcesso> { conta1, conta2 });
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta1.Id)).ReturnsAsync(usuario1);
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta2.Id)).ReturnsAsync(usuario2);

        var resultado = await _administradorService.ListarAdministradores();

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, a => a.Nome == "Admin Um" && a.Email == "admin1@doces.com");
        Assert.Contains(resultado, a => a.Nome == "Admin Dois" && a.Email == "admin2@doces.com");
    }

    [Fact]
    public async Task Dado_NenhumAdministrador_Quando_ListarAdministradores_Entao_DeveRetornarListaVazia()
    {
        _userManagerMock.Setup(u => u.GetUsersInRoleAsync("Administrador"))
            .ReturnsAsync(new List<ContaDeAcesso>());

        var resultado = await _administradorService.ListarAdministradores();

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_CadastrarAdministrador_Entao_DeveRepassarOPapelAdministrador()
    {
        var dto = new CadastroDTO
        {
            Nome = "Novo Admin",
            Email = "novo.admin@doces.com",
            Celular = "11987654321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "529.982.247-25",
            Senha = "SenhaForte@123"
        };

        await _administradorService.CadastrarAdministrador(dto);

        _usuarioServiceMock.Verify(s => s.CadastrarUsuario(dto, "Administrador"), Times.Once);
    }

    [Fact]
    public async Task Dado_PrincipalEUmDesativado_Quando_ListarAdministradores_Entao_DeveMarcarSituacaoEPrincipal()
    {
        var principal = CriarConta(AdministradorPrincipal.Email);
        var desativado = CriarConta("antigo@doces.com");
        desativado.LockoutEnd = AdministradorPrincipal.FimDaDesativacao;
        _userManagerMock.Setup(u => u.GetUsersInRoleAsync("Administrador"))
            .ReturnsAsync(new List<ContaDeAcesso> { principal, desativado });
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(principal.Id))
            .ReturnsAsync(new Usuario(principal.Id, "Principal", "52998224725", "11987654321", new DateTime(1990, 1, 1)));
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(desativado.Id))
            .ReturnsAsync(new Usuario(desativado.Id, "Antigo", "11144477735", "11987654322", new DateTime(1990, 1, 1)));

        var resultado = await _administradorService.ListarAdministradores();

        Assert.Contains(resultado, a => a.Email == AdministradorPrincipal.Email && a.Principal && a.Ativo);
        Assert.Contains(resultado, a => a.Email == "antigo@doces.com" && !a.Principal && !a.Ativo);
    }

    [Fact]
    public async Task Dado_BloqueioTemporarioPorSenha_Quando_ListarAdministradores_Entao_DeveContinuarAtivo()
    {
        var conta = CriarConta("errou.senha@doces.com");
        conta.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
        _userManagerMock.Setup(u => u.GetUsersInRoleAsync("Administrador")).ReturnsAsync(new List<ContaDeAcesso> { conta });
        _usuarioRepositoryMock.Setup(r => r.BuscarPorId(conta.Id))
            .ReturnsAsync(new Usuario(conta.Id, "Errou", "52998224725", "11987654321", new DateTime(1990, 1, 1)));

        var resultado = await _administradorService.ListarAdministradores();

        Assert.True(resultado.Single().Ativo);
    }

    [Fact]
    public async Task Dado_PrincipalEAlvoAdmin_Quando_Desativar_Entao_DeveTravarAContaAteOFimDaDesativacaoEInvalidarASessao()
    {
        var (principal, alvo) = PrepararPrincipalEAlvo();

        await _administradorService.Desativar(alvo.Id, principal.Id);

        _userManagerMock.Verify(u => u.SetLockoutEnabledAsync(alvo, true), Times.Once);
        _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(alvo, AdministradorPrincipal.FimDaDesativacao), Times.Once);
        _userManagerMock.Verify(u => u.UpdateSecurityStampAsync(alvo), Times.Once);
    }

    [Fact]
    public async Task Dado_PrincipalEAlvoDesativado_Quando_Reativar_Entao_DeveLimparOLockout()
    {
        var (principal, alvo) = PrepararPrincipalEAlvo();

        await _administradorService.Reativar(alvo.Id, principal.Id);

        _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(alvo, null), Times.Once);
        _userManagerMock.Verify(u => u.ResetAccessFailedCountAsync(alvo), Times.Once);
    }

    [Fact]
    public async Task Dado_ExecutorNaoPrincipal_Quando_Desativar_Entao_DeveLancarInvalidOperationExceptionSemTravar()
    {
        var (_, alvo) = PrepararPrincipalEAlvo();
        var comum = CriarConta("comum@doces.com");
        _userManagerMock.Setup(u => u.FindByIdAsync(comum.Id.ToString())).ReturnsAsync(comum);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _administradorService.Desativar(alvo.Id, comum.Id));

        _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(It.IsAny<ContaDeAcesso>(), It.IsAny<DateTimeOffset?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AlvoPrincipal_Quando_Desativar_Entao_DeveLancarInvalidOperationException()
    {
        var (principal, _) = PrepararPrincipalEAlvo();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _administradorService.Desativar(principal.Id, principal.Id));

        _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(It.IsAny<ContaDeAcesso>(), It.IsAny<DateTimeOffset?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AlvoQueNaoEAdmin_Quando_Desativar_Entao_DeveLancarKeyNotFoundException()
    {
        var (principal, alvo) = PrepararPrincipalEAlvo();
        _userManagerMock.Setup(u => u.IsInRoleAsync(alvo, "Administrador")).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _administradorService.Desativar(alvo.Id, principal.Id));
    }

    [Fact]
    public async Task Dado_AlvoInexistente_Quando_Reativar_Entao_DeveLancarKeyNotFoundException()
    {
        var (principal, _) = PrepararPrincipalEAlvo();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _administradorService.Reativar(Guid.NewGuid(), principal.Id));
    }

    [Fact]
    public async Task Dado_ContaDoPrincipal_Quando_EhPrincipal_Entao_DeveRetornarTrue()
    {
        var (principal, alvo) = PrepararPrincipalEAlvo();

        Assert.True(await _administradorService.EhPrincipal(principal.Id));
        Assert.False(await _administradorService.EhPrincipal(alvo.Id));
    }

    private (ContaDeAcesso Principal, ContaDeAcesso Alvo) PrepararPrincipalEAlvo()
    {
        var principal = CriarConta(AdministradorPrincipal.Email);
        var alvo = CriarConta("alvo@doces.com");

        _userManagerMock.Setup(u => u.FindByIdAsync(principal.Id.ToString())).ReturnsAsync(principal);
        _userManagerMock.Setup(u => u.FindByIdAsync(alvo.Id.ToString())).ReturnsAsync(alvo);
        _userManagerMock.Setup(u => u.IsInRoleAsync(It.IsAny<ContaDeAcesso>(), "Administrador")).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.SetLockoutEnabledAsync(It.IsAny<ContaDeAcesso>(), It.IsAny<bool>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.SetLockoutEndDateAsync(It.IsAny<ContaDeAcesso>(), It.IsAny<DateTimeOffset?>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.UpdateSecurityStampAsync(It.IsAny<ContaDeAcesso>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.ResetAccessFailedCountAsync(It.IsAny<ContaDeAcesso>())).ReturnsAsync(IdentityResult.Success);

        return (principal, alvo);
    }

    private static ContaDeAcesso CriarConta(string email)
    {
        var conta = new ContaDeAcesso(email);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());
        return conta;
    }
}
