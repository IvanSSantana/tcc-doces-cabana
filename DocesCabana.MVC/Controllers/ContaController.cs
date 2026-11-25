using System.Security.Claims;
using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

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
            return View("Index", dto);

        if (await _usuarioService.CpfEmUsoPorOutraConta(dto.CPF!, UsuarioAtualId))
        {
            ModelState.AddModelError(nameof(DadosPessoaisDTO.CPF), MensagensCadastro.DadosJaAssociados);
            return View("Index", dto);
        }

        await _usuarioService.AlterarDadosUsuario(new UsuarioDTO
        {
            Id = UsuarioAtualId,
            Nome = dto.Nome,
            Celular = dto.Celular,
            DataNascimento = dto.DataNascimento,
            CPF = dto.CPF!,
        });

        return RedirectToAction(nameof(Index));
    }

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

        if (await _enderecoService.EnderecoJaCadastrado(dto, UsuarioAtualId))
        {
            ModelState.AddModelError(nameof(EnderecoDTO.Numero), MensagensEndereco.EnderecoRepetido);
            return View("FormularioEndereco", dto);
        }

        await _enderecoService.Cadastrar(dto, UsuarioAtualId);

        return RedirectToAction(nameof(Enderecos));
    }

    [HttpGet]
    public async Task<IActionResult> EditarEndereco(Guid id)
    {
        var dto = await _enderecoService.BuscarDoUsuario(id, UsuarioAtualId);
        return View("FormularioEndereco", dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEndereco(EnderecoDTO dto)
    {
        if (!ModelState.IsValid)
            return View("FormularioEndereco", dto);

        if (await _enderecoService.EnderecoJaCadastrado(dto, UsuarioAtualId))
        {
            ModelState.AddModelError(nameof(EnderecoDTO.Numero), MensagensEndereco.EnderecoRepetido);
            return View("FormularioEndereco", dto);
        }

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

    private Guid UsuarioAtualId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
