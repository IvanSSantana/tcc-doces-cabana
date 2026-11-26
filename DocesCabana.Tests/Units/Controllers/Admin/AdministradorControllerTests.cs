using System.Security.Claims;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.MVC.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class AdministradorControllerTests
{
    private readonly Mock<IAdministradorService> _administradorServiceMock;
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly AdministradorController _controller;
    private readonly Guid _usuarioAtualId = Guid.NewGuid();

    public AdministradorControllerTests()
    {
        _administradorServiceMock = new Mock<IAdministradorService>();
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _controller = new AdministradorController(_administradorServiceMock.Object, _usuarioServiceMock.Object);
    }

    [Fact]
    public async Task Dado_AdministradoresCadastrados_Quando_Index_Entao_DeveRetornarViewComALista()
    {
        var administradores = new List<AdministradorDTO>
        {
            new() { Id = Guid.NewGuid(), Nome = "Admin Um", Email = "admin1@doces.com" }
        };
        _administradorServiceMock.Setup(s => s.ListarAdministradores())
            .ReturnsAsync(administradores);
        ConfigurarTempData();

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(administradores, viewResult.Model);
    }

    [Fact]
    public void Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveRetornarView()
    {
        var resultado = _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarViewComDtoSemChamarServico()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new CadastroDTO();

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        _administradorServiceMock.Verify(s => s.CadastrarAdministrador(It.IsAny<CadastroDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_CadastroPost_Entao_DeveCadastrarERedirecionarComConfirmacao()
    {
        var dto = new CadastroDTO
        {
            Nome = "Novo Admin",
            Email = "novo.admin@doces.com",
            Celular = "11987654321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "529.982.247-25",
            Senha = "SenhaForte@123",
            ConfirmacaoSenha = "SenhaForte@123"
        };
        _administradorServiceMock.Setup(s => s.CadastrarAdministrador(dto))
            .ReturnsAsync(new UsuarioDTO { Nome = dto.Nome!, Email = dto.Email! });

        ConfigurarTempData();

        var resultado = await _controller.Cadastro(dto);

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirectResult.ActionName);
        _administradorServiceMock.Verify(s => s.CadastrarAdministrador(dto), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_CpfJaUsado_Quando_CadastroPost_Entao_DeveRetornarViewComErroSemCadastrar()
    {
        var dto = new CadastroDTO
        {
            Nome = "Admin Repetido",
            Email = "admin.novo@doces.com",
            Celular = "11987654321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "529.982.247-25",
            Senha = "SenhaForte@123",
            ConfirmacaoSenha = "SenhaForte@123"
        };
        _usuarioServiceMock.Setup(s => s.ContaJaExiste(dto.Email!, dto.CPF!)).ReturnsAsync(true);

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        _administradorServiceMock.Verify(s => s.CadastrarAdministrador(It.IsAny<CadastroDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EmailJaUsado_Quando_CadastroPost_Entao_DeveRetornarViewComErroSemCadastrar()
    {
        var dto = new CadastroDTO
        {
            Nome = "Admin Repetido",
            Email = "admin.existente@doces.com",
            Celular = "11987654321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "111.444.777-35",
            Senha = "SenhaForte@123",
            ConfirmacaoSenha = "SenhaForte@123"
        };
        _usuarioServiceMock.Setup(s => s.ContaJaExiste(dto.Email!, dto.CPF!)).ReturnsAsync(true);

        var resultado = await _controller.Cadastro(dto);

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        _administradorServiceMock.Verify(s => s.CadastrarAdministrador(It.IsAny<CadastroDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipal_Quando_Index_Entao_DeveAvisarAViewQueEhPrincipal()
    {
        ConfigurarTempData();
        _administradorServiceMock.Setup(s => s.ListarAdministradores()).ReturnsAsync([]);
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(true, viewResult.ViewData["EhPrincipal"]);
    }

    [Fact]
    public async Task Dado_ExecutorNaoPrincipal_Quando_Desativar_Entao_DeveRetornarForbidSemChamarOServico()
    {
        ConfigurarTempData();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(false);

        var resultado = await _controller.Desativar(Guid.NewGuid());

        Assert.IsType<ForbidResult>(resultado);
        _administradorServiceMock.Verify(s => s.Desativar(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipal_Quando_Desativar_Entao_DeveDesativarERedirecionarComConfirmacao()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);

        var resultado = await _controller.Desativar(alvoId);

        _administradorServiceMock.Verify(s => s.Desativar(alvoId, _usuarioAtualId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Administrador desativado.", _controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_ExecutorNaoPrincipal_Quando_Reativar_Entao_DeveRetornarForbidSemChamarOServico()
    {
        ConfigurarTempData();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(false);

        var resultado = await _controller.Reativar(Guid.NewGuid());

        Assert.IsType<ForbidResult>(resultado);
        _administradorServiceMock.Verify(s => s.Reativar(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipal_Quando_Reativar_Entao_DeveReativarERedirecionarComConfirmacao()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);

        var resultado = await _controller.Reativar(alvoId);

        _administradorServiceMock.Verify(s => s.Reativar(alvoId, _usuarioAtualId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Administrador reativado.", _controller.TempData["Confirmacao"]);
    }

    [Theory]
    [InlineData(nameof(AdministradorController.Desativar))]
    [InlineData(nameof(AdministradorController.Reativar))]
    public void Dado_AcaoQueMudaEstado_Quando_OlharOsAtributos_Entao_DeveSerPostComAntiforgery(string acao)
    {
        var metodo = typeof(AdministradorController).GetMethod(acao)!;

        Assert.NotNull(metodo.GetCustomAttributes(typeof(HttpPostAttribute), false).SingleOrDefault());
        Assert.NotNull(metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).SingleOrDefault());
    }

    [Fact]
    public async Task Dado_ExecutorNaoPrincipal_Quando_EditarGet_Entao_DeveRetornarForbidSemBuscar()
    {
        ConfigurarTempData();

        var resultado = await _controller.Editar(Guid.NewGuid());

        Assert.IsType<ForbidResult>(resultado);
        _administradorServiceMock.Verify(s => s.BuscarAdministrador(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipal_Quando_EditarGet_Entao_DeveDevolverOFormularioPreenchido()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);
        _administradorServiceMock.Setup(s => s.BuscarAdministrador(alvoId, _usuarioAtualId)).ReturnsAsync(new UsuarioDTO
        {
            Id = alvoId, Nome = "Admin Dois", Email = "admin2@doces.com", Celular = "14999999999",
            DataNascimento = new DateTime(1990, 1, 1), CPF = "52998224725"
        });

        var resultado = await _controller.Editar(alvoId);

        var view = Assert.IsType<ViewResult>(resultado);
        var dto = Assert.IsType<DadosPessoaisDTO>(view.Model);
        Assert.Equal("Admin Dois", dto.Nome);
        Assert.Equal("admin2@doces.com", dto.Email);
        Assert.Equal("52998224725", dto.CPF);
    }

    [Fact]
    public async Task Dado_ExecutorNaoPrincipal_Quando_EditarPost_Entao_DeveRetornarForbidSemEditar()
    {
        ConfigurarTempData();

        var resultado = await _controller.Editar(Guid.NewGuid(), CriarDadosValidos());

        Assert.IsType<ForbidResult>(resultado);
        _administradorServiceMock.Verify(s => s.EditarAdministrador(It.IsAny<Guid>(), It.IsAny<UsuarioDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_EditarPost_Entao_DeveVoltarAViewSemEditar()
    {
        ConfigurarTempData();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);
        _controller.ModelState.AddModelError("Email", "O e-mail é inválido.");
        var dto = CriarDadosValidos();

        var resultado = await _controller.Editar(Guid.NewGuid(), dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Same(dto, view.Model);
        _administradorServiceMock.Verify(s => s.EditarAdministrador(It.IsAny<Guid>(), It.IsAny<UsuarioDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EmailDeOutraConta_Quando_EditarPost_Entao_DeveVoltarComErroNoEmailSemEditar()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);
        _usuarioServiceMock.Setup(s => s.EmailEmUsoPorOutraConta("admin2@doces.com", alvoId)).ReturnsAsync(true);

        var resultado = await _controller.Editar(alvoId, CriarDadosValidos());

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState["Email"]!.Errors.Single().ErrorMessage);
        _administradorServiceMock.Verify(s => s.EditarAdministrador(It.IsAny<Guid>(), It.IsAny<UsuarioDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CpfDeOutraConta_Quando_EditarPost_Entao_DeveVoltarComErroNoCpfSemEditar()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);
        _usuarioServiceMock.Setup(s => s.CpfEmUsoPorOutraConta("529.982.247-25", alvoId)).ReturnsAsync(true);

        var resultado = await _controller.Editar(alvoId, CriarDadosValidos());

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState["CPF"]!.Errors.Single().ErrorMessage);
        _administradorServiceMock.Verify(s => s.EditarAdministrador(It.IsAny<Guid>(), It.IsAny<UsuarioDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipalEDadosValidos_Quando_EditarPost_Entao_DeveEditarERedirecionarComConfirmacao()
    {
        ConfigurarTempData();
        var alvoId = Guid.NewGuid();
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioAtualId)).ReturnsAsync(true);

        var resultado = await _controller.Editar(alvoId, CriarDadosValidos());

        _administradorServiceMock.Verify(s => s.EditarAdministrador(alvoId, It.Is<UsuarioDTO>(
            u => u.Nome == "Admin Dois" && u.Email == "admin2@doces.com" && u.CPF == "529.982.247-25" && u.Celular == "(14) 99999-9999"),
            _usuarioAtualId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Administrador atualizado.", _controller.TempData["Confirmacao"]);
    }

    [Fact]
    public void Dado_EditarPost_Quando_OlharOsAtributos_Entao_DeveSerPostComAntiforgery()
    {
        var metodo = typeof(AdministradorController).GetMethod(
            nameof(AdministradorController.Editar), [typeof(Guid), typeof(DadosPessoaisDTO)])!;

        Assert.NotNull(metodo.GetCustomAttributes(typeof(HttpPostAttribute), false).SingleOrDefault());
        Assert.NotNull(metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).SingleOrDefault());
    }

    private static DadosPessoaisDTO CriarDadosValidos() => new()
    {
        Nome = "Admin Dois", Email = "admin2@doces.com", Celular = "(14) 99999-9999",
        DataNascimento = new DateTime(1990, 1, 1), CPF = "529.982.247-25"
    };

    private void ConfigurarTempData()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, _usuarioAtualId.ToString())], "TesteAutenticacao"))
        };
        var tempDataProviderMock = new Mock<ITempDataProvider>();
        tempDataProviderMock.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());

        _controller.TempData = new TempDataDictionary(httpContext, tempDataProviderMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
