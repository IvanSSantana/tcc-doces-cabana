using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Constants;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity.Services;

public class AdministradorService : IAdministradorService
{
    private readonly UserManager<ContaDeAcesso> _userManager;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUsuarioService _usuarioService;

    public AdministradorService(
        UserManager<ContaDeAcesso> userManager,
        IUsuarioRepository usuarioRepository,
        IUsuarioService usuarioService)
    {
        _userManager = userManager;
        _usuarioRepository = usuarioRepository;
        _usuarioService = usuarioService;
    }

    public async Task<List<AdministradorDTO>> ListarAdministradores()
    {
        var contas = await _userManager.GetUsersInRoleAsync(Papeis.Administrador);
        var administradores = new List<AdministradorDTO>();

        foreach (var conta in contas)
        {
            var usuario = await _usuarioRepository.BuscarPorId(conta.Id);
            if (usuario is not null)
                administradores.Add(AdministradorMapper.ToDTO(usuario, conta));
        }

        return administradores;
    }

    public Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto) =>
        _usuarioService.CadastrarUsuario(dto, Papeis.Administrador);

    public async Task<bool> EhPrincipal(Guid usuarioId)
    {
        var conta = await _userManager.FindByIdAsync(usuarioId.ToString());
        return conta is not null && conta.EhAdministradorPrincipal;
    }

    public async Task Desativar(Guid administradorId, Guid executorId)
    {
        var alvo = await BuscarAlvoOuFalhar(administradorId, executorId);

        Garantir(await _userManager.SetLockoutEnabledAsync(alvo, true));
        Garantir(await _userManager.SetLockoutEndDateAsync(alvo, AdministradorPrincipal.FimDaDesativacao));
        Garantir(await _userManager.UpdateSecurityStampAsync(alvo));
    }

    public async Task Reativar(Guid administradorId, Guid executorId)
    {
        var alvo = await BuscarAlvoOuFalhar(administradorId, executorId);

        Garantir(await _userManager.SetLockoutEndDateAsync(alvo, null));
        Garantir(await _userManager.ResetAccessFailedCountAsync(alvo));
    }

    /// <summary>
    /// Segunda barreira da autorização (spec 039, D7): o controller já recusa quem não é principal.
    /// </summary>
    private async Task<ContaDeAcesso> BuscarAlvoOuFalhar(Guid administradorId, Guid executorId)
    {
        if (!await EhPrincipal(executorId))
            throw new InvalidOperationException("Só o administrador principal pode desativar ou reativar administradores.");

        var alvo = await _userManager.FindByIdAsync(administradorId.ToString());
        if (alvo is null || !await _userManager.IsInRoleAsync(alvo, Papeis.Administrador))
            throw new KeyNotFoundException("Administrador não encontrado.");

        if (alvo.EhAdministradorPrincipal)
            throw new InvalidOperationException("O administrador principal não pode ser desativado.");

        return alvo;
    }

    private static void Garantir(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
    }
}
