using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

public class AutenticacaoController : Controller
{
    private readonly ILogger<AutenticacaoController> _logger;
    private readonly IUsuarioServices _usuarioServices;

    public AutenticacaoController(ILogger<AutenticacaoController> logger, IUsuarioServices usuarioServices)
    {
        _logger = logger;
        _usuarioServices = usuarioServices;
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

        var usuario = await _usuarioServices.BuscarPorLogin(dto.Login);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            // Pesquisar como consumir esse erro no frontend para exibir o log
            return View(dto);
        }

        var resultado = await _usuarioServices.RealizarLogin(usuario.Email!, dto.Senha, dto.LembrarMe);

        if (resultado.Succeeded)
            return RedirectToAction("Index", "Home");

        if (resultado.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Conta bloqueada. Tente novamente mais tarde.");

        else
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");

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

        try
        {
            await _usuarioServices.CadastrarUsuario(dto);
            return RedirectToAction("Login");
        }
        // Substituir por um middleware de tratamento global de exceções para evitar repetição de código
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar usuário");
            ModelState.AddModelError(string.Empty, "Erro ao realizar cadastro. Tente novamente.");
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _usuarioServices.RealizarLogout();
        return RedirectToAction("Login");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}