using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Constants;
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
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return NoContent();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Avaliar(NovaAvaliacaoDTO dto)
    {
        if (User.IsInRole(Papeis.Administrador))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var parcial = PartialView("_FormularioAvaliacao", dto);
            parcial.StatusCode = StatusCodes.Status400BadRequest;
            return parcial;
        }

        await _avaliacaoService.Avaliar(dto, UsuarioAtualId!.Value);

        TempData["ConfirmacaoAvaliacao"] = "Obrigado pela sua avaliação!";

        var url = Url.Action(nameof(Detalhes), new { id = dto.ProdutoId, ordenacao = OrdenacaoAvaliacao.MaisRecentes })!;
        return Redirect($"{url}#avaliacoes");
    }

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
