using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;

namespace DocesCabana.Infrastructure.Identity.Services;

public interface IAdministradorService
{
    Task<List<UsuarioDTO>> ListarAdministradores();

    Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto);
}
