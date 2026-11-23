using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class VendasController : Controller
{
    private readonly IPainelDeVendasService _painelDeVendasService;

    public VendasController(IPainelDeVendasService painelDeVendasService)
    {
        _painelDeVendasService = painelDeVendasService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(PeriodoDoPainel periodo = PeriodoDoPainel.Mes, DateOnly? data = null)
    {
        var painel = await _painelDeVendasService.Montar(periodo, data);

        return View(painel);
    }
}
