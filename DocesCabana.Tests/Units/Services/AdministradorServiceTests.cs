using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs.Autenticacao;
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

    private static ContaDeAcesso CriarConta(string email)
    {
        var conta = new ContaDeAcesso(email);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, Guid.NewGuid());
        return conta;
    }
}
