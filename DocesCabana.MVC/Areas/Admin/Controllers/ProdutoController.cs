using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Constants;
using DocesCabana.MVC.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class ProdutoController : Controller
{
    private const string FormularioDoProduto = "_FormularioProduto";

    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IArmazenamentoDeImagem _armazenamento;
    private readonly IValidator<ImagemParaEnvioDTO> _imagemValidator;

    public ProdutoController(
        IProdutoService produtoService, ICategoriaService categoriaService,
        IArmazenamentoDeImagem armazenamento, IValidator<ImagemParaEnvioDTO> imagemValidator)
    {
        _produtoService = produtoService;
        _categoriaService = categoriaService;
        _armazenamento = armazenamento;
        _imagemValidator = imagemValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(FiltroDeGestaoDeProduto filtro = FiltroDeGestaoDeProduto.Todos, int pagina = 1)
    {
        var paginaDeProdutos = await _produtoService.ListarParaGestao(filtro, pagina);

        var modelo = new GestaoDeProdutosViewModel { Pagina = paginaDeProdutos, Filtro = filtro };

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ResultadoDaGestao", modelo);

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhe(Guid id)
    {
        var produto = await _produtoService.BuscarProdutoPorId(id);
        await CarregarSubcategorias();

        return PartialView("_DetalheProduto", produto);
    }

    [HttpGet]
    public async Task<IActionResult> Cadastro()
    {
        await CarregarSubcategorias();
        return PartialView(FormularioDoProduto, new ProdutoDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(ProdutoDTO dto, IFormFile? imagem)
    {
        if (imagem is null || imagem.Length == 0)
            ModelState.AddModelError("imagem", "A imagem é obrigatória.");
        else
            await ValidarImagem(imagem);

        ModelState.Remove(nameof(ProdutoDTO.ImagemUrl));

        if (!ModelState.IsValid)
            return await FormularioComErro(dto);

        var resultadoDoEnvio = await _armazenamento.Enviar(imagem!.OpenReadStream(), imagem.FileName, imagem.ContentType);
        if (!resultadoDoEnvio.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultadoDoEnvio.Mensagem!);
            return await FormularioComErro(dto);
        }

        await _produtoService.Cadastrar(dto.ComImagem(resultadoDoEnvio.Url!));

        TempData["Confirmacao"] = "Produto cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edicao(Guid id)
    {
        var produto = await _produtoService.BuscarProdutoPorId(id);
        await CarregarSubcategorias();

        return PartialView(FormularioDoProduto, produto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edicao(ProdutoDTO dto, IFormFile? imagem)
    {
        var atual = await _produtoService.BuscarProdutoPorId(dto.ProdutoId);
        dto = dto.ComImagem(atual.ImagemUrl);

        var enviouImagem = imagem is not null && imagem.Length > 0;
        if (enviouImagem)
            await ValidarImagem(imagem!);

        ModelState.Remove(nameof(ProdutoDTO.ImagemUrl));

        if (!ModelState.IsValid)
            return await FormularioComErro(dto);

        if (enviouImagem)
        {
            var resultadoDoEnvio = await _armazenamento.Enviar(imagem!.OpenReadStream(), imagem.FileName, imagem.ContentType);
            if (!resultadoDoEnvio.Sucesso)
            {
                ModelState.AddModelError(string.Empty, resultadoDoEnvio.Mensagem!);
                return await FormularioComErro(dto);
            }

            dto = dto.ComImagem(resultadoDoEnvio.Url!);
        }

        await _produtoService.Atualizar(dto);

        TempData["Confirmacao"] = "Produto atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(Guid id, FiltroDeGestaoDeProduto filtro, int pagina)
    {
        await _produtoService.Inativar(id);

        TempData["Confirmacao"] = "Produto inativado. Ele não aparece mais na loja.";
        return RedirectToAction(nameof(Index), new { filtro, pagina });
    }

    private async Task ValidarImagem(IFormFile imagem)
    {
        var metadados = new ImagemParaEnvioDTO(imagem.FileName, imagem.ContentType, imagem.Length);
        var validacao = await _imagemValidator.ValidateAsync(metadados);
        if (!validacao.IsValid)
            ModelState.AddModelError("imagem", validacao.Errors[0].ErrorMessage);
    }

    private async Task<IActionResult> FormularioComErro(ProdutoDTO dto)
    {
        await CarregarSubcategorias();

        var parcial = PartialView(FormularioDoProduto, dto);
        parcial.StatusCode = StatusCodes.Status400BadRequest;
        return parcial;
    }

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
}
