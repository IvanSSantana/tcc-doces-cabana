using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

// [Authorize] na classe (RF-02): quem chega aqui já passou pelo passo de
// conta — não existe visitante fechando pedido.
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

    // Gravar é do pedido; exibir é do carrinho (plano §1) — por isso, tanto
    // no sucesso quanto na recusa, quem decide o que aparece na tela
    // continua sendo a view de Carrinho/Index. O redirecionamento no
    // sucesso é o que resolve o CA-14 sozinho: recarregar o comprovante é
    // um GET, e nenhum segundo pedido nasce.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fechar(FechamentoDePedidoDTO dados)
    {
        if (ModelState.IsValid)
        {
            // Divergência, item indisponível e entrega incalculável são
            // erro esperado do usuário (RN-02/RN-06) — nunca exceção
            // (Princípio VIII), então não há try/catch aqui.
            var resultado = await _pedidoService.Fechar(UsuarioAtualId, dados);

            if (resultado.Sucesso)
                return RedirectToAction(nameof(Confirmacao), new { id = resultado.PedidoId });

            ModelState.AddModelError(string.Empty, resultado.Mensagem ?? "Não foi possível concluir o pedido. Tente novamente.");
            ViewData["ValorDosProdutosAtual"] = resultado.ValorDosProdutosAtual;
            ViewData["ValorDoFreteAtual"] = resultado.ValorDoFreteAtual;
            ViewData["ItemIndisponivel"] = resultado.ItemIndisponivel;
        }

        // RF-15/CA-16/CA-17: a tela reexibe com os valores atuais — o
        // formulário postado nunca é gravado (RN-02), só provoca reexibição.
        var carrinho = await _carrinhoService.ObterDoUsuario(UsuarioAtualId);
        ViewData["PassoDoFechamento"] = await _pedidoService.MontarPasso(
            PassoDoFechamento.Pagamento, carrinho, UsuarioAtualId, dados.EnderecoId);

        return View("~/Views/Carrinho/Index.cshtml", carrinho);
    }

    [HttpGet]
    public async Task<IActionResult> Confirmacao(Guid id)
    {
        // RN-08: pedido alheio (ou inexistente) devolve o mesmo 404, sem
        // distinguir os dois casos — IPedidoService.ObterConfirmacao já
        // resolve isso, sem try/catch aqui.
        var confirmacao = await _pedidoService.ObterConfirmacao(id, UsuarioAtualId);
        if (confirmacao is null)
            return NotFound();

        return View(confirmacao);
    }

    // Sempre autenticado — [Authorize] na classe garante que a claim existe.
    private Guid UsuarioAtualId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
