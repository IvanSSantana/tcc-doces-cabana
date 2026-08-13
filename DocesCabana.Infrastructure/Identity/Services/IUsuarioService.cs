using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity.Services;

public interface IUsuarioService
{
    Task<UsuarioDTO> CadastrarUsuario(CadastroDTO usuario, string? papel = null);
    Task<UsuarioDTO> BuscarUsuarioPorId(Guid usuarioId);
    Task<UsuarioDTO?> BuscarPorLogin(string login);
    Task<UsuarioDTO> AlterarDadosUsuario(UsuarioDTO usuarioDto);
    Task<SignInResult> RealizarLogin(string login, string senha, bool lembrarMe);
    Task RealizarLogout();
    Task<string> GerarTokenRedefinicaoSenha(string email);
    Task<bool> SolicitarRedefinicaoSenha(string email, string corpo);
    Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha);
    Task<bool> ConfirmarEmailDoUsuario(string email, string token);
}
