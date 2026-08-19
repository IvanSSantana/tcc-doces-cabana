using System.Diagnostics;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain;
using DocesCabana.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

// Era Controllers/CatalogoController.cs (010). Renomeado por RQ-04 da 011:
// gerencia produto, não catálogo — "catálogo" é a coleção que o cliente
// percorre (spec 012), e precisava do nome livre.
[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class ProdutoController : Controller
{
    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;

    public ProdutoController(IProdutoService produtoService, ICategoriaService categoriaService)
    {
        _produtoService = produtoService;
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> Cadastro()
    {
        await CarregarSubcategorias();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(ProdutoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            await CarregarSubcategorias();
            return View(dto);
        }

        await _produtoService.Cadastrar(dto);

        TempData["Confirmacao"] = "Produto cadastrado com sucesso!";
        return RedirectToAction(nameof(Cadastro));
    }

    // RF-28: cada opção mostra a categoria dona da subcategoria — sem isso,
    // "Cappuccino" aparece duas vezes no seletor sem nenhuma forma de saber
    // qual é a de Doces e qual é a de Empório (spec 012 §10).
    private async Task CarregarSubcategorias()
    {
        var categorias = await _categoriaService.ListarComSubcategorias();

        var opcoes = categorias
            .SelectMany(categoria => categoria.Subcategorias.Select(subcategoria => new
            {
                subcategoria.SubcategoriaId,
                Rotulo = $"{categoria.Nome} › {subcategoria.Nome}"
            }))
            .OrderBy(o => o.Rotulo);

        ViewBag.Subcategorias = new SelectList(opcoes, "SubcategoriaId", "Rotulo");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
