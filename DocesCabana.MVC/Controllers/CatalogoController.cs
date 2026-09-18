using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

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
