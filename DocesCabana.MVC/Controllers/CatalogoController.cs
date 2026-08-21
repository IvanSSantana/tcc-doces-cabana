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
        [FromQuery] Guid[]? subcategorias = null,
        [FromQuery] bool semAcucar = false,
        OrdenacaoCatalogo ordenacao = OrdenacaoCatalogo.MelhorAvaliados,
        int pagina = 1)
    {
        var filtro = new FiltroCatalogoDTO(
            CategoriaId: null,
            SubcategoriaIds: subcategorias ?? [],
            ApenasSemAcucar: semAcucar,
            Ordenacao: SanearOrdenacao(ordenacao));

        var catalogo = await _catalogoService.Montar(apelido, filtro, pagina, UsuarioAtualId);

        // Um endereço, duas representações (spec 014, plano §5): a mesma
        // rota devolve só o bloco que mudou para quem pediu via catalogo.js,
        // e a página inteira para quem navegou por link, F5 ou sem
        // JavaScript algum. Nenhuma rota nova, nenhuma duplicação de regra
        // de filtro/ordenação/paginação entre as duas.
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ResultadoCatalogo", catalogo);

        return View(catalogo);
    }

    // RN-07: "Mais vendidos" é anunciada, não oferecida — o ligador de
    // modelo aceita o valor por vir de um enum válido, mas o controller
    // recusa executar essa ordenação até a spec 019 dar sentido a ela.
    private static OrdenacaoCatalogo SanearOrdenacao(OrdenacaoCatalogo ordenacao) =>
        ordenacao == OrdenacaoCatalogo.MaisVendidos
            ? OrdenacaoCatalogo.MelhorAvaliados
            : ordenacao;

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
