using DocesCabana.MVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DocesCabana.Tests.Units.Controllers;

public class InstitucionalControllerTests
{
    private readonly InstitucionalController _controller = new();

    [Fact]
    public void Dado_RequisicaoValida_Quando_Privacidade_Entao_DeveRetornarView()
    {
        var resultado = _controller.Privacidade();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public void Dado_RequisicaoValida_Quando_QuemSomos_Entao_DeveRetornarView()
    {
        var resultado = _controller.QuemSomos();

        Assert.IsType<ViewResult>(resultado);
    }
}
