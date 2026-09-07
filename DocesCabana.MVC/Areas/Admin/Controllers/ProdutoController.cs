using System.Diagnostics;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain;
using DocesCabana.MVC.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public async Task<IActionResult> Cadastro()
    {
        await CarregarSubcategorias();
        return View();
    }

    // imagem substitui o campo de endereço (spec 027, RF-01): o formulário
    // envia o arquivo, e quem preenche ImagemUrl é o servidor, depois do
    // envio (passo 6). Por isso o teste de existência da própria coluna
    // (Princípio III) segue no domínio e no ProdutoDTOValidator, mas o
    // binding automático não pode mais decidir a invalidez desse campo — ver
    // o ModelState.Remove abaixo.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(ProdutoDTO dto, IFormFile? imagem)
    {
        if (imagem is null || imagem.Length == 0)
        {
            ModelState.AddModelError("imagem", "A imagem é obrigatória.");
        }
        else
        {
            var metadados = new ImagemParaEnvioDTO(imagem.FileName, imagem.ContentType, imagem.Length);
            var validacao = await _imagemValidator.ValidateAsync(metadados);
            if (!validacao.IsValid)
                ModelState.AddModelError("imagem", validacao.Errors[0].ErrorMessage);
        }

        // ImagemUrl não é mais campo de formulário — nunca veio do que a
        // pessoa preencheu, e sim do resultado do envio (passo 6). O erro
        // que o binding levanta sobre ele não descreve nada de real: mesma
        // situação do CPF em ContaController.AlterarDados (CA-07, spec 018).
        ModelState.Remove(nameof(ProdutoDTO.ImagemUrl));

        if (!ModelState.IsValid)
        {
            await CarregarSubcategorias();
            return View(dto);
        }

        var resultadoDoEnvio = await _armazenamento.Enviar(imagem!.OpenReadStream(), imagem.FileName, imagem.ContentType);
        if (!resultadoDoEnvio.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultadoDoEnvio.Mensagem!);
            await CarregarSubcategorias();
            return View(dto);
        }

        dto = dto.ComImagem(resultadoDoEnvio.Url!);
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
