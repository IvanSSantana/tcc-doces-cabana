using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Constants;
using DocesCabana.MVC.Areas.Admin.Controllers;
using DocesCabana.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class RetiradaControllerTests
{
    private static readonly DateOnly Segunda = new(2026, 11, 23);

    private readonly Mock<IAgendaDeRetiradaService> _agendaServiceMock = new();
    private readonly RetiradaController _controller;
    private readonly AgendaDeRetiradaDTO _agendaSalva;

    public RetiradaControllerTests()
    {
        _agendaSalva = new AgendaDeRetiradaDTO
        {
            TempoDePreparoEmMinutos = 60,
            Horarios = Enum.GetValues<DayOfWeek>().Select(d => new HorarioDeRetiradaDTO { DiaDaSemana = d }).ToList(),
            DiasBloqueados = [new DiaSemRetiradaDTO { Data = Segunda, Motivo = "Feriado" }]
        };
        _agendaServiceMock.Setup(s => s.ObterParaGestao()).ReturnsAsync(_agendaSalva);

        var contextoHttp = new DefaultHttpContext();
        _controller = new RetiradaController(_agendaServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public void Dado_Controller_Quando_InspecionarAtributos_Entao_DeveExigirPapelDeAdministrador()
    {
        var autorizacao = typeof(RetiradaController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(Papeis.Administrador, autorizacao.Roles);
    }

    [Theory]
    [InlineData(nameof(RetiradaController.SalvarHorarios))]
    [InlineData(nameof(RetiradaController.BloquearDia))]
    [InlineData(nameof(RetiradaController.DesbloquearDia))]
    public void Dado_AcaoDeEscrita_Quando_InspecionarAtributos_Entao_DeveSerPostComAntiforgery(string acao)
    {
        var metodo = typeof(RetiradaController).GetMethod(acao)!;

        Assert.NotNull(metodo.GetCustomAttributes(typeof(HttpPostAttribute), false).SingleOrDefault());
        Assert.NotNull(metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).SingleOrDefault());
    }

    [Fact]
    public async Task Dado_Requisicao_Quando_Index_Entao_DeveDevolverAgendaEFormularioDeBloqueioVazio()
    {
        var resultado = await _controller.Index();

        var modelo = Assert.IsType<AgendaDeRetiradaViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.Same(_agendaSalva, modelo.Agenda);
        Assert.Null(modelo.NovoBloqueio.Data);
    }

    [Fact]
    public async Task Dado_HorariosValidos_Quando_SalvarHorarios_Entao_DeveSalvarERedirecionar()
    {
        var dto = new AgendaDeRetiradaDTO { TempoDePreparoEmMinutos = 30 };

        var resultado = await _controller.SalvarHorarios(dto);

        _agendaServiceMock.Verify(s => s.SalvarHorarios(dto), Times.Once);
        Assert.Equal(nameof(RetiradaController.Index), Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_SalvarHorarios_Entao_DeveDevolverIndexComOPostadoEOsBloqueiosSalvos()
    {
        var dto = new AgendaDeRetiradaDTO { TempoDePreparoEmMinutos = 5 };
        _controller.ModelState.AddModelError("Agenda.TempoDePreparoEmMinutos", "Inválida");

        var resultado = await _controller.SalvarHorarios(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(nameof(RetiradaController.Index), view.ViewName);
        var modelo = Assert.IsType<AgendaDeRetiradaViewModel>(view.Model);
        Assert.Same(dto, modelo.Agenda);
        Assert.Single(modelo.Agenda.DiasBloqueados);
        _agendaServiceMock.Verify(s => s.SalvarHorarios(It.IsAny<AgendaDeRetiradaDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DataNova_Quando_BloquearDia_Entao_DeveBloquearERedirecionar()
    {
        var dto = new DiaSemRetiradaDTO { Data = Segunda.AddDays(1) };

        var resultado = await _controller.BloquearDia(dto);

        _agendaServiceMock.Verify(s => s.BloquearDia(dto), Times.Once);
        Assert.Equal(nameof(RetiradaController.Index), Assert.IsType<RedirectToActionResult>(resultado).ActionName);
    }

    [Fact]
    public async Task Dado_DataJaBloqueada_Quando_BloquearDia_Entao_DeveDevolverErroNoCampo()
    {
        var dto = new DiaSemRetiradaDTO { Data = Segunda };
        _agendaServiceMock.Setup(s => s.DiaJaBloqueado(Segunda)).ReturnsAsync(true);

        var resultado = await _controller.BloquearDia(dto);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(nameof(RetiradaController.Index), view.ViewName);
        Assert.True(_controller.ModelState.ContainsKey("NovoBloqueio.Data"));
        var modelo = Assert.IsType<AgendaDeRetiradaViewModel>(view.Model);
        Assert.Same(dto, modelo.NovoBloqueio);
        Assert.Same(_agendaSalva, modelo.Agenda);
        _agendaServiceMock.Verify(s => s.BloquearDia(It.IsAny<DiaSemRetiradaDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_BloquearDia_Entao_NaoDeveConsultarNemBloquear()
    {
        _controller.ModelState.AddModelError("NovoBloqueio.Data", "Informe a data.");

        var resultado = await _controller.BloquearDia(new DiaSemRetiradaDTO());

        Assert.IsType<ViewResult>(resultado);
        _agendaServiceMock.Verify(s => s.DiaJaBloqueado(It.IsAny<DateOnly>()), Times.Never);
        _agendaServiceMock.Verify(s => s.BloquearDia(It.IsAny<DiaSemRetiradaDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DataBloqueada_Quando_DesbloquearDia_Entao_DeveDesbloquearERedirecionar()
    {
        var resultado = await _controller.DesbloquearDia(Segunda);

        _agendaServiceMock.Verify(s => s.DesbloquearDia(Segunda), Times.Once);
        Assert.Equal(nameof(RetiradaController.Index), Assert.IsType<RedirectToActionResult>(resultado).ActionName);
    }
}
