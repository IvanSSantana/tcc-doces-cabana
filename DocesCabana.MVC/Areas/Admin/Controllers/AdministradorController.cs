using System.Security.Claims;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Constants;
using DocesCabana.Domain.Constants;
using DocesCabana.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
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
        ViewData["EhPrincipal"] = await _administradorService.EhPrincipal(UsuarioAtualId);
        return View(administradores);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid id)
    {
        if (!await _administradorService.EhPrincipal(UsuarioAtualId))
            return Forbid();

        await _administradorService.Desativar(id, UsuarioAtualId);

        TempData["Confirmacao"] = "Administrador desativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(Guid id)
    {
        if (!await _administradorService.EhPrincipal(UsuarioAtualId))
            return Forbid();

        await _administradorService.Reativar(id, UsuarioAtualId);

        TempData["Confirmacao"] = "Administrador reativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        if (!await _administradorService.EhPrincipal(UsuarioAtualId))
            return Forbid();

        var administrador = await _administradorService.BuscarAdministrador(id, UsuarioAtualId);

        return View(new DadosPessoaisDTO
        {
            Nome = administrador.Nome,
            Email = administrador.Email,
            Celular = administrador.Celular,
            DataNascimento = administrador.DataNascimento,
            CPF = administrador.CPF,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, DadosPessoaisDTO dto)
    {
        if (!await _administradorService.EhPrincipal(UsuarioAtualId))
            return Forbid();

        if (!ModelState.IsValid)
            return View(dto);

        if (await _usuarioService.CpfEmUsoPorOutraConta(dto.CPF!, id))
        {
            ModelState.AddModelError(nameof(DadosPessoaisDTO.CPF), MensagensCadastro.DadosJaAssociados);
            return View(dto);
        }

        if (await _usuarioService.EmailEmUsoPorOutraConta(dto.Email.Trim(), id))
        {
            ModelState.AddModelError(nameof(DadosPessoaisDTO.Email), MensagensCadastro.DadosJaAssociados);
            return View(dto);
        }

        await _administradorService.EditarAdministrador(id, new UsuarioDTO
        {
            Nome = dto.Nome,
            Email = dto.Email.Trim(),
            Celular = dto.Celular,
            DataNascimento = dto.DataNascimento,
            CPF = dto.CPF!,
        }, UsuarioAtualId);

        TempData["Confirmacao"] = "Administrador atualizado.";
        return RedirectToAction(nameof(Index));
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

    private Guid UsuarioAtualId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
