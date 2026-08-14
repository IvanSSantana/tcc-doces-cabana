using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

public class ProdutoController : Controller
{
    private readonly IProdutoService _produtoService;
    private readonly IAvaliacaoService _avaliacaoService;

    public ProdutoController(IProdutoService produtoService, IAvaliacaoService avaliacaoService)
    {
        _produtoService = produtoService;
        _avaliacaoService = avaliacaoService;
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, OrdenacaoAvaliacao ordenacao = OrdenacaoAvaliacao.Relevantes, int exibir = 5)
    {
        exibir = SanearExibir(exibir);

        var detalhe = await _produtoService.BuscarDetalhe(id, ordenacao, exibir, UsuarioAtualId);

        return View(detalhe);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> VotarUtil(Guid avaliacaoId, OrdenacaoAvaliacao ordenacao, int exibir)
    {
        var produtoId = await _avaliacaoService.AlternarVotoUtil(avaliacaoId, UsuarioAtualId!.Value);

        var url = Url.Action(nameof(Detalhes), new { id = produtoId, ordenacao, exibir })!;
        return Redirect($"{url}#avaliacoes");
    }

    // Mínimo 5, máximo 100, arredondado para baixo em múltiplos de 5 —
    // impede que a query string vire carga arbitrária (plano §4, risco §8).
    private static int SanearExibir(int exibir)
    {
        var limitado = Math.Clamp(exibir, 5, 100);
        return limitado - (limitado % 5);
    }

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
