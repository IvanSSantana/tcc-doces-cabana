using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;

namespace DocesCabana.Application.Contracts.Services;

public interface IAdministradorService
{
    Task<List<AdministradorDTO>> ListarAdministradores();

    Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto);

    Task<bool> EhPrincipal(Guid usuarioId);

    Task Desativar(Guid administradorId, Guid executorId);

    Task Reativar(Guid administradorId, Guid executorId);
}
