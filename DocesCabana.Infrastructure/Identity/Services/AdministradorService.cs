using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain;
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

    public async Task<List<UsuarioDTO>> ListarAdministradores()
    {
        var contas = await _userManager.GetUsersInRoleAsync(Papeis.Administrador);
        var administradores = new List<UsuarioDTO>();

        foreach (var conta in contas)
        {
            var usuario = await _usuarioRepository.BuscarPorId(conta.Id);
            if (usuario is not null)
                administradores.Add(UsuarioMapper.ToDTO(usuario, conta));
        }

        return administradores;
    }

    public Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto) =>
        _usuarioService.CadastrarUsuario(dto, Papeis.Administrador);
}
