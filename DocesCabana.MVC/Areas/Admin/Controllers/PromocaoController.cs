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
public class PromocaoController : Controller
{
    private const string PrefixoDoFormulario = nameof(FormularioDePromocaoViewModel.Promocao);

    private readonly IPromocaoService _promocaoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IProdutoService _produtoService;

    public PromocaoController(
        IPromocaoService promocaoService, ICategoriaService categoriaService, IProdutoService produtoService)
    {
        _promocaoService = promocaoService;
        _categoriaService = categoriaService;
        _produtoService = produtoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var promocoes = await _promocaoService.ListarParaGestao();

        return View(promocoes);
    }

    [HttpGet]
    public async Task<IActionResult> Cadastro() =>
        View(await MontarFormulario(new PromocaoDTO
        {
            Tipo = TipoDeDesconto.Percentual,
            Abrangencia = AbrangenciaDaPromocao.ProdutosEscolhidos
        }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro([Bind(Prefix = PrefixoDoFormulario)] PromocaoDTO dto)
    {
        if (!ModelState.IsValid)
            return View(await MontarFormulario(dto));

        await _promocaoService.Cadastrar(dto);

        TempData["Confirmacao"] = "Promoção cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edicao(Guid id)
    {
        var promocao = await _promocaoService.BuscarParaEdicao(id);

        return View(await MontarFormulario(promocao));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edicao([Bind(Prefix = PrefixoDoFormulario)] PromocaoDTO dto)
    {
        if (!ModelState.IsValid)
            return View(await MontarFormulario(dto));

        await _promocaoService.Atualizar(dto);

        TempData["Confirmacao"] = "Promoção atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ativar(Guid id)
    {
        await _promocaoService.Ativar(id);

        TempData["Confirmacao"] = "Promoção ativada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _promocaoService.Desativar(id);

        TempData["Confirmacao"] = "Promoção desativada. Os pedidos já feitos não mudam.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<FormularioDePromocaoViewModel> MontarFormulario(PromocaoDTO dto)
    {
        var categorias = await _categoriaService.ListarComSubcategorias();
        var produtos = await _produtoService.BuscarTodosProdutos();

        return new FormularioDePromocaoViewModel
        {
            Promocao = dto,
            Categorias = categorias,
            Produtos = produtos
                .Where(p => p.Status == ProdutoStatus.Ativo)
                .OrderBy(p => p.Nome)
                .ToList()
        };
    }
}
