using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Helpers;

namespace DocesCabana.MVC.Controllers;

public class CarrinhoController : Controller
{
    private readonly ICarrinhoService _carrinhoService;
    private readonly IFreteService _freteService;
    private readonly IValidator<ConsultaDeFreteDTO> _consultaDeFreteValidator;
    private readonly IPedidoService _pedidoService;
    private readonly IEnderecoService _enderecoService;

    public CarrinhoController(
        ICarrinhoService carrinhoService, IFreteService freteService,
        IValidator<ConsultaDeFreteDTO> consultaDeFreteValidator, IPedidoService pedidoService,
        IEnderecoService enderecoService)
    {
        _carrinhoService = carrinhoService;
        _freteService = freteService;
        _consultaDeFreteValidator = consultaDeFreteValidator;
        _pedidoService = pedidoService;
        _enderecoService = enderecoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? cep = null, PassoDoFechamento passo = PassoDoFechamento.Carrinho,
        Guid? enderecoId = null, int? servicoDeEntregaId = null)
    {
        var carrinho = await ObterCarrinhoAtual();

        if (!string.IsNullOrWhiteSpace(cep))
        {
            var consulta = new ConsultaDeFreteDTO(cep);
            var validacao = await _consultaDeFreteValidator.ValidateAsync(consulta);

            if (!validacao.IsValid)
            {
                validacao.AddToModelState(ModelState);
            }
            else
            {
                var disponiveis = carrinho.Linhas.Where(l => l.Disponivel).ToList();
                if (disponiveis.Count > 0)
                {
                    var cotacao = await _freteService.Cotar(cep, disponiveis);
                    carrinho = carrinho.ComCotacao(cotacao);
                }
            }
        }

        await MontarPassoDoFechamento(carrinho, passo, enderecoId, servicoDeEntregaId);

        if (EhRequisicaoAssincrona)
            return PartialView("_ItensDoCarrinho", carrinho);

        return View(carrinho);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CadastrarEndereco(EnderecoDTO dto)
    {
        if (ModelState.IsValid)
            await _enderecoService.Cadastrar(dto, UsuarioAtualId!.Value);

        if (EhRequisicaoAssincrona)
        {
            var carrinho = await ObterCarrinhoAtual();
            await MontarPassoDoFechamento(carrinho, PassoDoFechamento.Endereco, null, null);
            return PartialView("_ItensDoCarrinho", carrinho);
        }

        return RedirectToAction(nameof(Index), new { passo = PassoDoFechamento.Endereco });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acrescentar(Guid produtoId, short quantidade = 1)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.Acrescentar(usuarioId.Value, produtoId, quantidade);
        }
        else
        {
            var atualizado = await _carrinhoService.AcrescentarAvulso(HttpContext.Session.Ler(), produtoId, quantidade);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarQuantidade(Guid produtoId, short quantidade)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.AlterarQuantidade(usuarioId.Value, produtoId, quantidade);
        }
        else
        {
            var atualizado = _carrinhoService.AlterarQuantidadeAvulsa(HttpContext.Session.Ler(), produtoId, quantidade);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid produtoId)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.Remover(usuarioId.Value, produtoId);
        }
        else
        {
            var atualizado = _carrinhoService.RemoverAvulso(HttpContext.Session.Ler(), produtoId);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    [HttpGet]
    public IActionResult ConfirmarEsvaziar() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Esvaziar()
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.Esvaziar(usuarioId.Value);
        }
        else
        {
            HttpContext.Session.Limpar();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<CarrinhoDTO> ObterCarrinhoAtual()
    {
        var usuarioId = UsuarioAtualId;
        return usuarioId is not null
            ? await _carrinhoService.ObterDoUsuario(usuarioId.Value)
            : await _carrinhoService.MontarAvulso(HttpContext.Session.Ler());
    }

    private async Task<IActionResult> DevolverResultado()
    {
        if (EhRequisicaoAssincrona)
        {
            var carrinho = await ObterCarrinhoAtual();
            await MontarPassoDoFechamento(carrinho, PassoDoFechamento.Carrinho, null, null);
            return PartialView("_ItensDoCarrinho", carrinho);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task MontarPassoDoFechamento(CarrinhoDTO carrinho, PassoDoFechamento passo, Guid? enderecoId, int? servicoDeEntregaId)
    {
        var usuarioId = UsuarioAtualId;

        var passoEfetivo = usuarioId is null && passo is PassoDoFechamento.Endereco or PassoDoFechamento.Pagamento
            ? PassoDoFechamento.Conta
            : passo;

        ViewData["PassoDoFechamento"] =
            await _pedidoService.MontarPasso(passoEfetivo, carrinho, usuarioId, enderecoId, servicoDeEntregaId);
    }

    private bool EhRequisicaoAssincrona =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest";

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
