using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

[Authorize(Roles = Papeis.Administrador)]
public class AdministradorController : Controller
{
    private readonly IAdministradorService _administradorService;

    public AdministradorController(IAdministradorService administradorService)
    {
        _administradorService = administradorService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var administradores = await _administradorService.ListarAdministradores();
        return View(administradores);
    }

    [HttpGet]
    public IActionResult Cadastro()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _administradorService.CadastrarAdministrador(dto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }

        TempData["Confirmacao"] = "Administrador cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}
