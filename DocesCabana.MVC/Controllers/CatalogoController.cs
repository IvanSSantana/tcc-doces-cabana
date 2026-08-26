using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

// Público, sem [Authorize] — o catálogo é a coleção que o cliente percorre
// (spec 012). O nome ficou livre a partir da spec 011.
public class CatalogoController : Controller
{
    private readonly ICatalogoService _catalogoService;

    public CatalogoController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? apelido = null,
        // Apelido, não Guid (spec 016): o valor que chega pela URL é
        // legível — "barras", "potes" — e é CatalogoService.Montar quem
        // resolve isso contra a categoria atual.
        [FromQuery] string[]? subcategorias = null,
        [FromQuery] bool semAcucar = false,
        OrdenacaoCatalogo ordenacao = OrdenacaoCatalogo.MelhorAvaliados,
        string? termo = null,
        int pagina = 1)
    {
        var criterios = new CriteriosDoCatalogoDTO(
            ApelidoDaCategoria: apelido,
            ApelidosDeSubcategoria: subcategorias ?? [],
            ApenasSemAcucar: semAcucar,
            Ordenacao: ordenacao,
            Termo: termo);

        var catalogo = await _catalogoService.Montar(criterios, pagina, UsuarioAtualId);

        // Um endereço, duas representações (spec 014, plano §5): a mesma
        // rota devolve só o bloco que mudou para quem pediu via catalogo.js,
        // e a página inteira para quem navegou por link, F5 ou sem
        // JavaScript algum. Nenhuma rota nova, nenhuma duplicação de regra
        // de filtro/ordenação/paginação entre as duas.
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ResultadoCatalogo", catalogo);

        return View(catalogo);
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
