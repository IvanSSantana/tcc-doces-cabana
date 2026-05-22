using DocesCabana.Application.DTOs.Auth;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Services;

public interface IUsuarioServices
{
    Usuario CadastrarUsuario(CadastroDTO usuario);
    Usuario BuscarUsuarioPorId(Guid usuarioId);
    bool RedefinirSenhaUsuario(Usuario usuario, string novaSenha);
    Usuario AlterarDadosUsuario(Usuario usuario);
}
    