using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Models;
using DocesCabana.Application.Contracts.Services;

namespace DocesCabana.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IProdutoService _produtoService;

    public HomeController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    public async Task<IActionResult> Index()
    {
        var produtos = await _produtoService.BuscarTodosProdutos();
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
}
