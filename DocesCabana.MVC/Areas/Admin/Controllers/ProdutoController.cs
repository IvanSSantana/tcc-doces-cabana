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
    private readonly ISubcategoriaService _subcategoriaService;

    public ProdutoController(IProdutoService produtoService, ISubcategoriaService subcategoriaService)
    {
        _produtoService = produtoService;
        _subcategoriaService = subcategoriaService;
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

    private async Task CarregarSubcategorias()
    {
        var subcategorias = await _subcategoriaService.BuscarTodasSubcategorias();
        ViewBag.Subcategorias = new SelectList(subcategorias, nameof(SubcategoriaDTO.SubcategoriaId), nameof(SubcategoriaDTO.Nome));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
