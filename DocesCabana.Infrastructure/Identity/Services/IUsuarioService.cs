using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity.Services;

public interface IUsuarioService
{
    Task<UsuarioDTO> CadastrarUsuario(CadastroDTO usuario);
    Task<UsuarioDTO> BuscarUsuarioPorId(Guid usuarioId);
    Task<UsuarioDTO?> BuscarPorLogin(string login);
    Task<bool> RedefinirSenhaUsuario(UsuarioDTO usuarioDto, string novaSenha);
    Task<UsuarioDTO> AlterarDadosUsuario(UsuarioDTO usuarioDto);
    Task<SignInResult> RealizarLogin(string login, string senha, bool lembrarMe);
    Task RealizarLogout();
    Task<bool> SolicitarRedefinicaoSenha(string email);
    Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha);
    Task<bool> ConfirmarEmailDoUsuario(string email, string token);
}
