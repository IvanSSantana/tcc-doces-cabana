using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class UsuarioServiceTests
{
    private readonly Mock<UserManager<Usuario>> _userManagerMock;
    private readonly Mock<SignInManager<Usuario>> _signInManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<UsuarioService>> _loggerMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceTests()
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

        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<UsuarioService>>();

        _usuarioService = new UsuarioService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_CadastrarUsuario_Entao_DeveRetornarUsuarioDto()
    {
        var dto = CriarCadastroDTO();
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), dto.Senha!))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _usuarioService.CadastrarUsuario(dto);

        Assert.NotNull(resultado);
        Assert.Equal(dto.Nome, resultado.Nome);
        Assert.Equal(dto.Email, resultado.Email);
    }

    [Fact]
    public async Task Dado_DadosDuplicados_Quando_CadastrarUsuario_Entao_DeveLancarInvalidOperationException()
    {
        var dto = CriarCadastroDTO();
        var erro = new IdentityError { Code = "DuplicateEmail", Description = "Email duplicado" };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), dto.Senha!))
            .ReturnsAsync(IdentityResult.Failed(erro));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto));

        Assert.Equal("Os dados informados já estão associados a uma conta existente.", ex.Message);
    }

    [Fact]
    public async Task Dado_FalhaGeralDoIdentity_Quando_CadastrarUsuario_Entao_DeveLancarInvalidOperationException()
    {
        var dto = CriarCadastroDTO();
        var erro = new IdentityError { Code = "GenericError", Description = "Erro genérico no cadastro" };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), dto.Senha!))
            .ReturnsAsync(IdentityResult.Failed(erro));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.CadastrarUsuario(dto));

        Assert.Contains("Erro genérico no cadastro", ex.Message);
    }

    [Fact]
    public async Task Dado_IdExistente_Quando_BuscarUsuarioPorId_Entao_DeveRetornarUsuarioDto()
    {
        var id = Guid.NewGuid();
        var usuario = CriarUsuario(id);
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
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
            .ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.BuscarUsuarioPorId(id));
    }

    [Fact]
    public async Task Dado_EmailExistente_Quando_BuscarPorLogin_Entao_DeveRetornarUsuarioDto()
    {
        var email = "teste@exemplo.com";
        var usuario = CriarUsuario(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
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
        var usuario = CriarUsuario(Guid.NewGuid(), cpf: cpfLimpo);

        _userManagerMock.Setup(u => u.FindByEmailAsync(login))
            .ReturnsAsync((Usuario?)null);

        var usuarios = new List<Usuario> { usuario };
        var mockQueryable = new TestAsyncEnumerable<Usuario>(usuarios);

        _userManagerMock.Setup(u => u.Users)
            .Returns(mockQueryable);

        var resultado = await _usuarioService.BuscarPorLogin(login);

        Assert.NotNull(resultado);
        Assert.Equal(cpfLimpo, resultado.CPF);
    }

    [Fact]
    public async Task Dado_LoginInexistente_Quando_BuscarPorLogin_Entao_DeveRetornarNull()
    {
        var login = "inexistente@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(login))
            .ReturnsAsync((Usuario?)null);

        var mockQueryable = new TestAsyncEnumerable<Usuario>(new List<Usuario>());
        _userManagerMock.Setup(u => u.Users)
            .Returns(mockQueryable);

        var resultado = await _usuarioService.BuscarPorLogin(login);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_AlterarDadosUsuario_Entao_DeveRetornarUsuarioDtoAtualizado()
    {
        var id = Guid.NewGuid();
        var usuario = CriarUsuario(id);
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Success);

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
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_AlterarDadosUsuario_Entao_DeveLancarKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync((Usuario?)null);

        var dto = new UsuarioDTO { Id = id };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.AlterarDadosUsuario(dto));
    }

    [Fact]
    public async Task Dado_FalhaIdentity_Quando_AlterarDadosUsuario_Entao_DeveLancarInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var usuario = CriarUsuario(id);
        _userManagerMock.Setup(u => u.FindByIdAsync(id.ToString()))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro ao atualizar" }));

        var dto = new UsuarioDTO
        {
            Id = id,
            Nome = "Novo Nome",
            Celular = "11988888888",
            DataNascimento = new DateTime(1995, 5, 5)
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _usuarioService.AlterarDadosUsuario(dto));

        Assert.Contains("Erro ao atualizar", ex.Message);
    }

    // Os testes de RealizarLogin (sucesso por e-mail, sucesso por CPF, login
    // inexistente e política de bloqueio) vivem em UsuarioServiceLoginTests.cs.

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
        var usuario = CriarUsuario(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(usuario))
            .ReturnsAsync("token-valido");

        var token = await _usuarioService.GerarTokenRedefinicaoSenha(email);

        Assert.Equal("token-valido", token);
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_GerarTokenRedefinicaoSenha_Entao_DeveLancarKeyNotFoundException()
    {
        var email = "senha@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usuarioService.GerarTokenRedefinicaoSenha(email));
    }

    [Fact]
    public async Task Dado_UsuarioInexistente_Quando_SolicitarRedefinicaoSenha_Entao_DeveRetornarFalse()
    {
        var email = "senha@email.com";
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _usuarioService.SolicitarRedefinicaoSenha(email, "corpo");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_UsuarioExistente_Quando_SolicitarRedefinicaoSenha_Entao_DeveEnviarEmailERetornarTrue()
    {
        var email = "senha@email.com";
        var usuario = CriarUsuario(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
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
            .ReturnsAsync((Usuario?)null);

        var resultado = await _usuarioService.ConfirmarRedefinicaoSenha(email, "token", "novaSenha");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_TokenValido_Quando_ConfirmarRedefinicaoSenha_Entao_DeveRetornarTrue()
    {
        var email = "senha@email.com";
        var usuario = CriarUsuario(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.ResetPasswordAsync(usuario, "token", "novaSenha"))
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
            .ReturnsAsync((Usuario?)null);

        var resultado = await _usuarioService.ConfirmarEmailDoUsuario(email, "token");

        Assert.False(resultado);
    }

    [Fact]
    public async Task Dado_TokenValido_Quando_ConfirmarEmailDoUsuario_Entao_DeveRetornarTrue()
    {
        var email = "email@email.com";
        var usuario = CriarUsuario(Guid.NewGuid(), email);
        _userManagerMock.Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.ConfirmEmailAsync(usuario, "token"))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _usuarioService.ConfirmarEmailDoUsuario(email, "token");

        Assert.True(resultado);
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

    private static Usuario CriarUsuario(Guid id, string email = "teste@exemplo.com", string cpf = "54839427011")
    {
        var usuario = new Usuario("Cliente Teste", email, "11999999999", new DateTime(1990, 1, 1), cpf);
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(usuario, id);
        return usuario;
    }
}

// Light-weight test helpers to mock async queries in EF Core (UserManager.Users.FirstOrDefaultAsync)
public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = _inner.Execute(expression);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}
