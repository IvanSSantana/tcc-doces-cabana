using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.MVC.Controllers;

public class AutenticacaoController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public AutenticacaoController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado = await _usuarioService.RealizarLogin(dto.Login!, dto.Senha!, dto.LembrarMe ?? false);

        if (resultado.Succeeded)
            return RedirectToAction("Index", "Home");

        if (resultado.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Conta bloqueada. Tente novamente mais tarde.");
        else
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorreto(s).");

        return View(dto);
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

        var usuarioExistente = await _usuarioService.BuscarPorLogin(dto.Email!) ?? await _usuarioService.BuscarPorLogin(dto.CPF!);

        if (usuarioExistente != null)
        {
            ModelState.AddModelError(string.Empty, "Os dados informados já estão associados a uma conta existente.");
            return View(dto);
        }

        await _usuarioService.CadastrarUsuario(dto);
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _usuarioService.RealizarLogout();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult EsqueceuSenha()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueceuSenha(EsqueceuSenhaDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var usuario = await _usuarioService.BuscarPorLogin(dto.Login);

        if (usuario != null)
        {
            var token = await _usuarioService.GerarTokenRedefinicaoSenha(usuario.Email!);
            var link = Url.Action("RedefinirSenha", "Autenticacao", new { token, usuario.Email }, Request.Scheme)!;
            var corpo =
                $@"<div>
                    Link para redefinir senha: <a href='{link}'>{link}</a>
                </div>";

            await _usuarioService.SolicitarRedefinicaoSenha(usuario.Email!, corpo);
        }

        TempData["Confirmacao"] = "Se existir uma conta com esse login, enviamos um e-mail com o link de redefinição.";
        return RedirectToAction(nameof(EsqueceuSenha));
    }

    [HttpGet]
    public IActionResult RedefinirSenha(string token, string email)
    {
        var dto = new RedefinirSenhaDTO { Token = token, Email = email };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var resultado = await _usuarioService.ConfirmarRedefinicaoSenha(dto.Email!, dto.Token!, dto.Senha);

        if (resultado)
            return RedirectToAction("Login", "Autenticacao");
        else
            ModelState.AddModelError(string.Empty, "Erro ao redefinir senha.");
        return View(dto);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
