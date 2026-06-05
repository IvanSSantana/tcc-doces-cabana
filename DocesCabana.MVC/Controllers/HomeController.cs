using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Models;
using DocesCabana.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;

namespace DocesCabana.MVC.Controllers;

[Authorize]
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
