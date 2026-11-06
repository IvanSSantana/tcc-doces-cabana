using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;

namespace DocesCabana.Application.Contracts.Services;

public interface IAdministradorService
{
    Task<List<UsuarioDTO>> ListarAdministradores();

    Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto);
}
