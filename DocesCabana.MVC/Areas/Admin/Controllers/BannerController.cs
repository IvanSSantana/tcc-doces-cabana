using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Constants;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class BannerController : Controller
{
    private const string PrefixoDoFormulario = nameof(FormularioDeBannerViewModel.Banner);

    private readonly IBannerService _bannerService;
    private readonly ICategoriaService _categoriaService;
    private readonly IProdutoService _produtoService;
    private readonly IArmazenamentoDeImagem _armazenamento;
    private readonly IValidator<ImagemParaEnvioDTO> _imagemValidator;

    public BannerController(
        IBannerService bannerService, ICategoriaService categoriaService, IProdutoService produtoService,
        IArmazenamentoDeImagem armazenamento, IValidator<ImagemParaEnvioDTO> imagemValidator)
    {
        _bannerService = bannerService;
        _categoriaService = categoriaService;
        _produtoService = produtoService;
        _armazenamento = armazenamento;
        _imagemValidator = imagemValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var banners = await _bannerService.ListarParaGestao();

        return View(banners);
    }

    [HttpGet]
    public async Task<IActionResult> Cadastro() =>
        View(await MontarFormulario(new BannerDTO { Ordem = 0, TipoDeDestino = TipoDeDestinoDoBanner.Categoria }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro([Bind(Prefix = PrefixoDoFormulario)] BannerDTO dto, IFormFile? imagem)
    {
        if (imagem is null || imagem.Length == 0)
            ModelState.AddModelError("imagem", "A imagem é obrigatória.");
        else
            await ValidarImagem(imagem);

        if (!ModelState.IsValid)
            return View(await MontarFormulario(dto));

        var resultadoDoEnvio = await _armazenamento.Enviar(imagem!.OpenReadStream(), imagem.FileName, imagem.ContentType);
        if (!resultadoDoEnvio.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultadoDoEnvio.Mensagem!);
            return View(await MontarFormulario(dto));
        }

        await _bannerService.Cadastrar(dto, resultadoDoEnvio.Url!);

        TempData["Confirmacao"] = "Banner cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edicao(Guid id)
    {
        var banner = await _bannerService.BuscarParaEdicao(id);

        return View(await MontarFormulario(banner));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edicao([Bind(Prefix = PrefixoDoFormulario)] BannerDTO dto, IFormFile? imagem)
    {
        var enviouImagem = imagem is not null && imagem.Length > 0;
        if (enviouImagem)
            await ValidarImagem(imagem!);

        if (!ModelState.IsValid)
            return View(await MontarFormularioDeEdicao(dto));

        string? novaImagemUrl = null;
        if (enviouImagem)
        {
            var resultadoDoEnvio = await _armazenamento.Enviar(imagem!.OpenReadStream(), imagem.FileName, imagem.ContentType);
            if (!resultadoDoEnvio.Sucesso)
            {
                ModelState.AddModelError(string.Empty, resultadoDoEnvio.Mensagem!);
                return View(await MontarFormularioDeEdicao(dto));
            }

            novaImagemUrl = resultadoDoEnvio.Url;
        }

        await _bannerService.Atualizar(dto, novaImagemUrl);

        TempData["Confirmacao"] = "Banner atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ativar(Guid id)
    {
        await _bannerService.Ativar(id);

        TempData["Confirmacao"] = "Banner ativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _bannerService.Desativar(id);

        TempData["Confirmacao"] = "Banner desativado. Ele saiu do carrossel.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarImagem(IFormFile imagem)
    {
        var metadados = new ImagemParaEnvioDTO(imagem.FileName, imagem.ContentType, imagem.Length);
        var validacao = await _imagemValidator.ValidateAsync(metadados);
        if (!validacao.IsValid)
            ModelState.AddModelError("imagem", validacao.Errors[0].ErrorMessage);
    }

    /// <summary>
    /// A imagem atual não volta no POST; é relida para o formulário com erro continuar mostrando-a.
    /// </summary>
    private async Task<FormularioDeBannerViewModel> MontarFormularioDeEdicao(BannerDTO dto)
    {
        var atual = await _bannerService.BuscarParaEdicao(dto.BannerId);
        dto.ImagemUrl = atual.ImagemUrl;

        return await MontarFormulario(dto);
    }

    private async Task<FormularioDeBannerViewModel> MontarFormulario(BannerDTO dto)
    {
        var categorias = await _categoriaService.ListarComSubcategorias();
        var produtos = await _produtoService.BuscarTodosProdutos();

        return new FormularioDeBannerViewModel
        {
            Banner = dto,
            Categorias = categorias,
            Produtos = produtos
                .Where(p => p.Status != ProdutoStatus.Inativo)
                .OrderBy(p => p.Nome)
                .ToList()
        };
    }
}
