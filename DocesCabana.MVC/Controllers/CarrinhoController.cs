using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Helpers;

namespace DocesCabana.MVC.Controllers;

// Duas fontes, uma tela: autenticado lê e escreve no banco; visitante, na
// sessão (spec 017, Fase 6). Nenhuma das quatro ações exige [Authorize] —
// cada uma decide por conta própria, olhando UsuarioAtualId, para atender
// os dois públicos com o mesmo endpoint.
public class CarrinhoController : Controller
{
    private readonly ICarrinhoService _carrinhoService;

    public CarrinhoController(ICarrinhoService carrinhoService)
    {
        _carrinhoService = carrinhoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var carrinho = await ObterCarrinhoAtual();

        if (EhRequisicaoAssincrona)
            return PartialView("_ItensDoCarrinho", carrinho);

        return View(carrinho);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acrescentar(Guid produtoId, short quantidade = 1)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            // Produto indisponível (RN-06) lança InvalidOperationException,
            // que o FilterException global captura e traduz — não há
            // try/catch aqui (Princípio VIII).
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

    // RF-11 (spec 021): a pergunta é a única tela que funciona sem
    // JavaScript — com script, o mesmo POST é disparado por um diálogo
    // inline (carrinho.js), sem passar por esta view.
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

    // Um endereço, duas representações (mesmo padrão da 014): o caminho
    // assíncrono recebe só o bloco que mudou; o comum, POST-Redirect-Get
    // de volta para a própria tela (Princípio VII).
    private async Task<IActionResult> DevolverResultado()
    {
        if (EhRequisicaoAssincrona)
        {
            var carrinho = await ObterCarrinhoAtual();
            return PartialView("_ItensDoCarrinho", carrinho);
        }

        return RedirectToAction(nameof(Index));
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
