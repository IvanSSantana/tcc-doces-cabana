using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Constants;
using DocesCabana.MVC.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class VendasControllerTests
{
    private readonly Mock<IPainelDeVendasService> _painelServiceMock = new();
    private readonly VendasController _controller;

    public VendasControllerTests()
    {
        _controller = new VendasController(_painelServiceMock.Object);
    }

    private static PainelDeVendasDTO PainelVazio(PeriodoDoPainel periodo) => new(
        periodo, new DateOnly(2026, 11, 1), "novembro de 2026", new DateOnly(2026, 10, 1), null,
        new(0m, 0m, null), new(0m, 0m, null), new(0m, 0m, null), [], [], [], []);

    [Fact]
    public void Dado_Controller_Quando_InspecionarAtributos_Entao_DeveExigirPapelDeAdministradorEAreaAdmin()
    {
        var autorizacao = typeof(VendasController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single();
        var area = typeof(VendasController)
            .GetCustomAttributes(typeof(AreaAttribute), false)
            .Cast<AreaAttribute>()
            .Single();

        Assert.Equal(Papeis.Administrador, autorizacao.Roles);
        Assert.Equal("Admin", area.RouteValue);
    }

    [Fact]
    public async Task Dado_SemFiltro_Quando_Index_Entao_DeveMontarOMesCorrenteEDevolverAView()
    {
        var painel = PainelVazio(PeriodoDoPainel.Mes);
        _painelServiceMock.Setup(s => s.Montar(PeriodoDoPainel.Mes, null)).ReturnsAsync(painel);

        var resultado = await _controller.Index();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Same(painel, view.Model);
    }

    [Fact]
    public async Task Dado_PeriodoEData_Quando_Index_Entao_DevePassarOsDoisAoServico()
    {
        var data = new DateOnly(2026, 11, 12);
        _painelServiceMock.Setup(s => s.Montar(PeriodoDoPainel.Semana, data)).ReturnsAsync(PainelVazio(PeriodoDoPainel.Semana));

        await _controller.Index(PeriodoDoPainel.Semana, data);

        _painelServiceMock.Verify(s => s.Montar(PeriodoDoPainel.Semana, data), Times.Once);
    }

    [Fact]
    public void Dado_AcaoIndex_Quando_InspecionarAtributos_Entao_DeveSerSoGet()
    {
        var metodo = typeof(VendasController).GetMethod(nameof(VendasController.Index))!;

        Assert.NotNull(metodo.GetCustomAttributes(typeof(HttpGetAttribute), false).SingleOrDefault());
    }
}
