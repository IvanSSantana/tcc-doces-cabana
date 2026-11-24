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
    private readonly IBannerService _bannerService;

    public HomeController(IProdutoService produtoService, IBannerService bannerService)
    {
        _produtoService = produtoService;
        _bannerService = bannerService;
    }

    public async Task<IActionResult> Index()
    {
        var produtos = await _produtoService.BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, UsuarioAtualId);
        ViewData["Banners"] = await _bannerService.ListarVigentesDoCarrossel();

        return View(produtos);
    }

    public IActionResult AcessoNegado()
    {
        return View();
    }
    
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
