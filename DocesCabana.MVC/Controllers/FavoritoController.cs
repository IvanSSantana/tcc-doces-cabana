using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

public class FavoritoController : Controller
{
    private readonly IFavoritoService _favoritoService;

    public FavoritoController(IFavoritoService favoritoService)
    {
        _favoritoService = favoritoService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var favoritos = await _favoritoService.ListarDoUsuario(UsuarioAtualId!.Value);
        return View(favoritos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(Guid produtoId, string? returnUrl = null)
    {
        var usuarioId = UsuarioAtualId;

        if (usuarioId is null)
        {
            if (EhRequisicaoAssincrona)
                return StatusCode(StatusCodes.Status401Unauthorized, new { autenticado = false });

            return RedirectToAction("Login", "Autenticacao", new { returnUrl });
        }

        var favoritado = await _favoritoService.Alternar(produtoId, usuarioId.Value);

        if (EhRequisicaoAssincrona)
            return Json(new { favoritado });

        if (returnUrl is not null && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    private bool EhRequisicaoAssincrona =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest";

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
