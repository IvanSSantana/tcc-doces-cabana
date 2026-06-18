using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

public class AutenticacaoController : Controller
{
    private readonly ILogger<AutenticacaoController> _logger;
    private readonly IUsuarioService _usuarioService;

    public AutenticacaoController(ILogger<AutenticacaoController> logger, IUsuarioService usuarioService)
    {
        _logger = logger;
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
        var usuario = await _usuarioService.BuscarPorLogin(dto.Login)!;

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Foi enviado um e-mail de confirmação caso a conta com esse login exista.");
            return View();
        }
            
        await _usuarioService.SolicitarRedefinicaoSenha(usuario!.Email!);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
