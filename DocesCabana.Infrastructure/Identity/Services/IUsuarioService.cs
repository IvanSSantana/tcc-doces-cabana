using DocesCabana.Application.DTOs.Autenticacao;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace DocesCabana.Infrastructure.Identity.Services;

public interface IUsuarioService
{
    Task<Usuario> CadastrarUsuario(CadastroDTO usuario);
    Task<Usuario> BuscarUsuarioPorId(Guid usuarioId);
    Task<Usuario?> BuscarPorLogin(string login);
    Task<bool> RedefinirSenhaUsuario(Usuario usuario, string novaSenha);
    Task<Usuario> AlterarDadosUsuario(Usuario usuario);
    Task<SignInResult> RealizarLogin(string login, string senha, bool lembrarMe);
    Task RealizarLogout();
    Task<bool> SolicitarRedefinicaoSenha(string email);
    Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha);
    Task<bool> EmailJaCadastrado(string email);
}
