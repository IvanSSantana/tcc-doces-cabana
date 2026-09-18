using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

[Authorize]
public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;
    private readonly ICarrinhoService _carrinhoService;

    public PedidoController(IPedidoService pedidoService, ICarrinhoService carrinhoService)
    {
        _pedidoService = pedidoService;
        _carrinhoService = carrinhoService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fechar(FechamentoDePedidoDTO dados)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _pedidoService.Fechar(UsuarioAtualId, dados);

            if (resultado.Sucesso)
                return RedirectToAction(nameof(Confirmacao), new { id = resultado.PedidoId });

            ModelState.AddModelError(string.Empty, resultado.Mensagem ?? "Não foi possível concluir o pedido. Tente novamente.");
            ViewData["ValorDosProdutosAtual"] = resultado.ValorDosProdutosAtual;
            ViewData["ValorDoFreteAtual"] = resultado.ValorDoFreteAtual;
            ViewData["ItemIndisponivel"] = resultado.ItemIndisponivel;
        }

        var carrinho = await _carrinhoService.ObterDoUsuario(UsuarioAtualId);
        ViewData["PassoDoFechamento"] = await _pedidoService.MontarPasso(
            PassoDoFechamento.Pagamento, carrinho, UsuarioAtualId, dados.EnderecoId);

        return View("~/Views/Carrinho/Index.cshtml", carrinho);
    }

    [HttpGet]
    public async Task<IActionResult> Confirmacao(Guid id)
    {
        var confirmacao = await _pedidoService.ObterConfirmacao(id, UsuarioAtualId);
        if (confirmacao is null)
            return NotFound();

        return View(confirmacao);
    }

    [HttpGet]
    public async Task<IActionResult> Meus()
    {
        var pedidos = await _pedidoService.ListarDoUsuario(UsuarioAtualId);
        return View(pedidos);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhe(Guid id)
    {
        var detalhe = await _pedidoService.BuscarDetalhe(id, UsuarioAtualId);
        return View(detalhe);
    }

    private Guid UsuarioAtualId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
