using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

// [Authorize] na classe (RF-03): a área de conta inteira exige autenticação
// — diferente do Carrinho/Favorito, aqui não existe visitante nenhum a
// atender.
[Authorize]
public class ContaController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IEnderecoService _enderecoService;

    public ContaController(IUsuarioService usuarioService, IEnderecoService enderecoService)
    {
        _usuarioService = usuarioService;
        _enderecoService = enderecoService;
    }

    // ── Dados pessoais ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuario = await _usuarioService.BuscarUsuarioPorId(UsuarioAtualId);

        return View(new DadosPessoaisDTO
        {
            Nome = usuario.Nome,
            Celular = usuario.Celular,
            DataNascimento = usuario.DataNascimento,
            CPF = usuario.CPF,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarDados(DadosPessoaisDTO dto)
    {
        if (!ModelState.IsValid)
        {
            // CA-07: o CPF continua visível mesmo quando o resto falhou —
            // ele nunca veio do que a pessoa digitou (não é campo de
            // formulário, RN-06), então precisa vir do que já está
            // guardado, não do DTO que voltou do POST.
            dto.CPF = (await _usuarioService.BuscarUsuarioPorId(UsuarioAtualId)).CPF;
            return View("Index", dto);
        }

        await _usuarioService.AlterarDadosUsuario(new UsuarioDTO
        {
            Id = UsuarioAtualId,
            Nome = dto.Nome,
            Celular = dto.Celular,
            DataNascimento = dto.DataNascimento,
        });

        return RedirectToAction(nameof(Index));
    }

    // ── Endereços ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Enderecos()
    {
        var enderecos = await _enderecoService.ListarDoUsuario(UsuarioAtualId);
        return View(enderecos);
    }

    [HttpGet]
    public IActionResult NovoEndereco() => View("FormularioEndereco", new EnderecoDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NovoEndereco(EnderecoDTO dto)
    {
        if (!ModelState.IsValid)
            return View("FormularioEndereco", dto);

        await _enderecoService.Cadastrar(dto, UsuarioAtualId);

        return RedirectToAction(nameof(Enderecos));
    }

    [HttpGet]
    public async Task<IActionResult> EditarEndereco(Guid id)
    {
        // Endereço alheio (ou inexistente) lança KeyNotFoundException, que
        // o FilterException global traduz para 404 — não há try/catch aqui
        // (Princípio VIII).
        var dto = await _enderecoService.BuscarDoUsuario(id, UsuarioAtualId);
        return View("FormularioEndereco", dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEndereco(EnderecoDTO dto)
    {
        if (!ModelState.IsValid)
            return View("FormularioEndereco", dto);

        await _enderecoService.Editar(dto, UsuarioAtualId);

        return RedirectToAction(nameof(Enderecos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirEndereco(Guid id)
    {
        await _enderecoService.Excluir(id, UsuarioAtualId);
        return RedirectToAction(nameof(Enderecos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TornarPrincipal(Guid id)
    {
        await _enderecoService.TornarPrincipal(id, UsuarioAtualId);
        return RedirectToAction(nameof(Enderecos));
    }

    // Sempre autenticado — [Authorize] na classe garante que a claim existe.
    private Guid UsuarioAtualId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
