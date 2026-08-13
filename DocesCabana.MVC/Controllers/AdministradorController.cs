using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Mensagens;
using DocesCabana.Domain;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

[Authorize(Roles = Papeis.Administrador)]
public class AdministradorController : Controller
{
    private readonly IAdministradorService _administradorService;
    private readonly IUsuarioService _usuarioService;

    public AdministradorController(IAdministradorService administradorService, IUsuarioService usuarioService)
    {
        _administradorService = administradorService;
        _usuarioService = usuarioService;
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

        if (await _usuarioService.ContaJaExiste(dto.Email!, dto.CPF!))
        {
            ModelState.AddModelError(string.Empty, MensagensCadastro.DadosJaAssociados);
            return View(dto);
        }

        await _administradorService.CadastrarAdministrador(dto);

        TempData["Confirmacao"] = "Administrador cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}
