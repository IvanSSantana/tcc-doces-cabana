using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Models;
using DocesCabana.MVC.ViewComponents;
using DocesCabana.Application.Contracts.Services;

namespace DocesCabana.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IProdutoService _produtoService;

    public HomeController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    // RF-04 (spec 019): pede só os destaques que a vitrine exibe, não a loja
    // inteira — mesmo limite que o componente aplicaria de qualquer forma.
    public async Task<IActionResult> Index()
    {
        var produtos = await _produtoService.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, UsuarioAtualId);
        return View(produtos);
    }

    public IActionResult AcessoNegado()
    {
        return View();
    }

    // Alvo de app.UseStatusCodePagesWithReExecute — qualquer 404 (produto
    // inexistente ou inativo, rota que não bate com nada) reexecuta aqui
    // (spec 008, RF-03/RF-04/CA-04/CA-05).
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NaoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private Guid? UsuarioAtualId
    {
        get
        {
            if (User.Identity is not { IsAuthenticated: true })
                return null;

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id is null ? null : Guid.Parse(id);
        }
    }
}
