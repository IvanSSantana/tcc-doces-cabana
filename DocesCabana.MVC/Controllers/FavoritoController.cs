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

    // Sem [Authorize] de propósito (spec 015, plano §8): um redirecionamento
    // de desafio do Identity devolveria 200 com o HTML da tela de login para
    // quem pede via fetch, e o script não teria como distinguir isso de
    // sucesso. Verificar aqui permite responder 401 à requisição assíncrona
    // e redirecionar de verdade na requisição comum.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(Guid produtoId, string? returnUrl = null)
    {
        var usuarioId = UsuarioAtualId;

        if (usuarioId is null)
        {
            // Não é Unauthorized() puro: aquele resultado não escreve corpo
            // nenhum, e app.UseStatusCodePagesWithReExecute (spec 008)
            // reexecuta qualquer resposta de erro sem corpo para a página de
            // "não encontrado" — o 401 virava 404 antes de chegar ao script,
            // que não tinha como distinguir "precisa entrar" de "sumiu".
            if (EhRequisicaoAssincrona)
                return StatusCode(StatusCodes.Status401Unauthorized, new { autenticado = false });

            return RedirectToAction("Login", "Autenticacao", new { returnUrl });
        }

        var favoritado = await _favoritoService.Alternar(produtoId, usuarioId.Value);

        if (EhRequisicaoAssincrona)
            return Json(new { favoritado });

        // POST-Redirect-Get (Princípio VII): a requisição comum é a única
        // que o navegador pode de fato recarregar, então é ela quem precisa
        // do redirecionamento — o caminho assíncrono não cria histórico.
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
