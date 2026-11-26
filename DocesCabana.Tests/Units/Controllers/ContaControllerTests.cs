using System.Security.Claims;
using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class ContaControllerTests
{
    private readonly Mock<IUsuarioService> _usuarioServiceMock;
    private readonly Mock<IEnderecoService> _enderecoServiceMock;
    private readonly Mock<IAdministradorService> _administradorServiceMock;
    private readonly ContaController _controller;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ContaControllerTests()
    {
        _usuarioServiceMock = new Mock<IUsuarioService>();
        _enderecoServiceMock = new Mock<IEnderecoService>();
        _administradorServiceMock = new Mock<IAdministradorService>();
        _controller = new ContaController(_usuarioServiceMock.Object, _enderecoServiceMock.Object, _administradorServiceMock.Object);

        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())], "TesteAutenticacao");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidade) }
        };
    }

    [Fact]
    public void Dado_OControlador_Quando_OlharAClasse_Entao_DeveExigirAutenticacao()
    {
        var exigeAutorizacao = typeof(ContaController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Any();

        Assert.True(exigeAutorizacao);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Index_Entao_DeveDevolverOsDadosDoUsuario()
    {
        var usuario = new UsuarioDTO { Id = _usuarioId, Nome = "Cliente Teste", Email = "cliente@teste.com", Celular = "(14) 99999-9999", CPF = "52998224725", DataNascimento = new DateTime(1994, 6, 6) };
        _usuarioServiceMock.Setup(s => s.BuscarUsuarioPorId(_usuarioId)).ReturnsAsync(usuario);

        var resultado = await _controller.Index();

        var view = Assert.IsType<ViewResult>(resultado);
        var dto = Assert.IsType<DadosPessoaisDTO>(view.Model);
        Assert.Equal("Cliente Teste", dto.Nome);
        Assert.Equal("52998224725", dto.CPF);
        Assert.Equal("cliente@teste.com", dto.Email);
        Assert.Equal(false, view.ViewData["EmailBloqueado"]);
    }

    [Fact]
    public async Task Dado_AdminPrincipal_Quando_Index_Entao_DeveBloquearOCampoDeEmail()
    {
        _usuarioServiceMock.Setup(s => s.BuscarUsuarioPorId(_usuarioId)).ReturnsAsync(new UsuarioDTO { Id = _usuarioId, Email = "admin@docescabana.com.br" });
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Index();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(true, view.ViewData["EmailBloqueado"]);
    }

    [Fact]
    public async Task Dado_EmailDeOutraConta_Quando_AlterarDados_Entao_DeveRetornarViewComErroNoEmail()
    {
        var dto = new DadosPessoaisDTO { Nome = "Nome", Email = "outro@teste.com", Celular = "(14) 98888-8888", DataNascimento = new DateTime(1994, 6, 6), CPF = "52998224725" };
        _usuarioServiceMock.Setup(s => s.EmailEmUsoPorOutraConta("outro@teste.com", _usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.AlterarDados(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState["Email"]!.Errors.Single().ErrorMessage);
        _usuarioServiceMock.Verify(s => s.AlterarDadosUsuario(It.IsAny<UsuarioDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_AdminPrincipalComOutroEmail_Quando_AlterarDados_Entao_DeveRetornarViewComErroNoEmail()
    {
        var dto = new DadosPessoaisDTO { Nome = "Nome", Email = "novo@teste.com", Celular = "(14) 98888-8888", DataNascimento = new DateTime(1994, 6, 6), CPF = "52998224725" };
        _administradorServiceMock.Setup(s => s.EhPrincipal(_usuarioId)).ReturnsAsync(true);
        _usuarioServiceMock.Setup(s => s.BuscarUsuarioPorId(_usuarioId)).ReturnsAsync(new UsuarioDTO { Id = _usuarioId, Email = "admin@docescabana.com.br" });

        var resultado = await _controller.AlterarDados(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(MensagensCadastro.EmailDoPrincipalImutavel, _controller.ModelState["Email"]!.Errors.Single().ErrorMessage);
        Assert.Equal(true, view.ViewData["EmailBloqueado"]);
        _usuarioServiceMock.Verify(s => s.AlterarDadosUsuario(It.IsAny<UsuarioDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_AlterarDados_Entao_NaoDeveChamarOServicoEDeveVoltarComOCpfDigitado()
    {
        _controller.ModelState.AddModelError("CPF", "CPF inválido.");

        var resultado = await _controller.AlterarDados(new DadosPessoaisDTO { Nome = "X", Celular = "123", DataNascimento = DateTime.UtcNow, CPF = "123" });

        var view = Assert.IsType<ViewResult>(resultado);
        var dto = Assert.IsType<DadosPessoaisDTO>(view.Model);
        Assert.Equal("123", dto.CPF);
        _usuarioServiceMock.Verify(s => s.AlterarDadosUsuario(It.IsAny<UsuarioDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CpfDeOutraConta_Quando_AlterarDados_Entao_DeveRetornarViewComErroNoCpf()
    {
        var dto = new DadosPessoaisDTO { Nome = "Nome", Celular = "(14) 98888-8888", DataNascimento = new DateTime(1994, 6, 6), CPF = "52998224725" };
        _usuarioServiceMock.Setup(s => s.CpfEmUsoPorOutraConta("52998224725", _usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.AlterarDados(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal(MensagensCadastro.DadosJaAssociados, _controller.ModelState["CPF"]!.Errors.Single().ErrorMessage);
        _usuarioServiceMock.Verify(s => s.AlterarDadosUsuario(It.IsAny<UsuarioDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DadosValidos_Quando_AlterarDados_Entao_DeveChamarOServicoERedirecionar()
    {
        var dto = new DadosPessoaisDTO { Nome = "Novo Nome", Email = "novo@teste.com", Celular = "(14) 98888-8888", DataNascimento = new DateTime(1994, 6, 6), CPF = "529.982.247-25" };
        _usuarioServiceMock.Setup(s => s.AlterarDadosUsuario(It.IsAny<UsuarioDTO>()))
            .ReturnsAsync(new UsuarioDTO { Id = _usuarioId });

        var resultado = await _controller.AlterarDados(dto);

        _usuarioServiceMock.Verify(s => s.AlterarDadosUsuario(It.Is<UsuarioDTO>(
            u => u.Id == _usuarioId && u.Nome == "Novo Nome" && u.Email == "novo@teste.com" && u.Celular == "(14) 98888-8888" && u.CPF == "529.982.247-25")), Times.Once);
        _usuarioServiceMock.Verify(s => s.RenovarSessao(_usuarioId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ContaController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Dado_UsuarioComEnderecos_Quando_Enderecos_Entao_DeveDevolverALista()
    {
        var lista = new List<EnderecoDTO> { new() { EnderecoId = Guid.NewGuid() } };
        _enderecoServiceMock.Setup(s => s.ListarDoUsuario(_usuarioId)).ReturnsAsync(lista);

        var resultado = await _controller.Enderecos();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Same(lista, view.Model);
    }

    [Fact]
    public void Dado_Chamar_Quando_NovoEndereco_Entao_DeveDevolverFormularioVazio()
    {
        var resultado = _controller.NovoEndereco();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        Assert.IsType<EnderecoDTO>(view.Model);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_NovoEnderecoPost_Entao_NaoDeveChamarOServico()
    {
        _controller.ModelState.AddModelError("CEP", "O CEP deve conter 8 dígitos.");

        var resultado = await _controller.NovoEndereco(new EnderecoDTO());

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        _enderecoServiceMock.Verify(s => s.Cadastrar(It.IsAny<EnderecoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EnderecoValido_Quando_NovoEnderecoPost_Entao_DeveCadastrarERedirecionar()
    {
        var dto = new EnderecoDTO();

        var resultado = await _controller.NovoEndereco(dto);

        _enderecoServiceMock.Verify(s => s.Cadastrar(dto, _usuarioId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ContaController.Enderecos), redirect.ActionName);
    }

    [Fact]
    public async Task Dado_EnderecoDoUsuario_Quando_EditarEnderecoGet_Entao_DeveDevolverOFormularioPreenchido()
    {
        var enderecoId = Guid.NewGuid();
        var dto = new EnderecoDTO { EnderecoId = enderecoId };
        _enderecoServiceMock.Setup(s => s.BuscarDoUsuario(enderecoId, _usuarioId)).ReturnsAsync(dto);

        var resultado = await _controller.EditarEndereco(enderecoId);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        Assert.Same(dto, view.Model);
    }

    [Fact]
    public async Task Dado_EnderecoDeOutraPessoa_Quando_EditarEnderecoGet_Entao_DevePropagarAExcecao()
    {
        var enderecoId = Guid.NewGuid();
        _enderecoServiceMock.Setup(s => s.BuscarDoUsuario(enderecoId, _usuarioId))
            .ThrowsAsync(new KeyNotFoundException("Endereço não encontrado."));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.EditarEndereco(enderecoId));
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_EditarEnderecoPost_Entao_NaoDeveChamarOServico()
    {
        _controller.ModelState.AddModelError("CEP", "O CEP deve conter 8 dígitos.");

        var resultado = await _controller.EditarEndereco(new EnderecoDTO { EnderecoId = Guid.NewGuid() });

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        _enderecoServiceMock.Verify(s => s.Editar(It.IsAny<EnderecoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EnderecoValido_Quando_EditarEnderecoPost_Entao_DeveEditarERedirecionar()
    {
        var dto = new EnderecoDTO { EnderecoId = Guid.NewGuid() };

        var resultado = await _controller.EditarEndereco(dto);

        _enderecoServiceMock.Verify(s => s.Editar(dto, _usuarioId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ContaController.Enderecos), redirect.ActionName);
    }

    [Fact]
    public async Task Dado_EnderecoRepetido_Quando_NovoEnderecoPost_Entao_DeveVoltarAViewComErroNoNumeroSemCadastrar()
    {
        var dto = new EnderecoDTO();
        _enderecoServiceMock.Setup(s => s.EnderecoJaCadastrado(dto, _usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.NovoEndereco(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        Assert.Equal(MensagensEndereco.EnderecoRepetido, _controller.ModelState[nameof(EnderecoDTO.Numero)]!.Errors.Single().ErrorMessage);
        _enderecoServiceMock.Verify(s => s.Cadastrar(It.IsAny<EnderecoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EnderecoRepetido_Quando_EditarEnderecoPost_Entao_DeveVoltarAViewComErroNoNumeroSemEditar()
    {
        var dto = new EnderecoDTO { EnderecoId = Guid.NewGuid() };
        _enderecoServiceMock.Setup(s => s.EnderecoJaCadastrado(dto, _usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.EditarEndereco(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal("FormularioEndereco", view.ViewName);
        Assert.True(_controller.ModelState.ContainsKey(nameof(EnderecoDTO.Numero)));
        _enderecoServiceMock.Verify(s => s.Editar(It.IsAny<EnderecoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_UmEnderecoId_Quando_ExcluirEndereco_Entao_DeveExcluirERedirecionar()
    {
        var enderecoId = Guid.NewGuid();

        var resultado = await _controller.ExcluirEndereco(enderecoId);

        _enderecoServiceMock.Verify(s => s.Excluir(enderecoId, _usuarioId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ContaController.Enderecos), redirect.ActionName);
    }

    [Fact]
    public async Task Dado_UmEnderecoId_Quando_TornarPrincipal_Entao_DeveTornarPrincipalERedirecionar()
    {
        var enderecoId = Guid.NewGuid();

        var resultado = await _controller.TornarPrincipal(enderecoId);

        _enderecoServiceMock.Verify(s => s.TornarPrincipal(enderecoId, _usuarioId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ContaController.Enderecos), redirect.ActionName);
    }
}
