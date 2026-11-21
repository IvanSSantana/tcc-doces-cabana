using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Constants;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class CupomController : Controller
{
    private const string PrefixoDoFormulario = nameof(FormularioDeCupomViewModel.Cupom);

    private readonly ICupomService _cupomService;
    private readonly ICategoriaService _categoriaService;

    public CupomController(ICupomService cupomService, ICategoriaService categoriaService)
    {
        _cupomService = cupomService;
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cupons = await _cupomService.ListarParaGestao();

        return View(cupons);
    }

    [HttpGet]
    public async Task<IActionResult> Cadastro() =>
        View(await MontarFormulario(new CupomDTO { Tipo = TipoDeDesconto.Percentual }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro([Bind(Prefix = PrefixoDoFormulario)] CupomDTO dto)
    {
        if (ModelState.IsValid && await _cupomService.CodigoJaExiste(dto.Codigo))
            ModelState.AddModelError($"{PrefixoDoFormulario}.{nameof(CupomDTO.Codigo)}", "Já existe um cupom com este código.");

        if (!ModelState.IsValid)
            return View(await MontarFormulario(dto));

        await _cupomService.Cadastrar(dto);

        TempData["Confirmacao"] = "Cupom cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edicao(Guid id)
    {
        var cupom = await _cupomService.BuscarParaEdicao(id);

        return View(await MontarFormulario(cupom));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edicao([Bind(Prefix = PrefixoDoFormulario)] CupomDTO dto)
    {
        if (!ModelState.IsValid)
            return View(await MontarFormulario(dto));

        await _cupomService.Atualizar(dto);

        TempData["Confirmacao"] = "Cupom atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ativar(Guid id)
    {
        await _cupomService.Ativar(id);

        TempData["Confirmacao"] = "Cupom ativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _cupomService.Desativar(id);

        TempData["Confirmacao"] = "Cupom desativado. Os pedidos já feitos não mudam.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<FormularioDeCupomViewModel> MontarFormulario(CupomDTO dto) => new()
    {
        Cupom = dto,
        Categorias = await _categoriaService.ListarComSubcategorias()
    };
}
