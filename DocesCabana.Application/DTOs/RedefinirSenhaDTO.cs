namespace DocesCabana.Application.DTOs.Autenticacao;

public class RedefinirSenhaDTO
{
    public Guid Id { get; set; }

    public string Token { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    public string ConfirmacaoSenha { get; set; } = string.Empty;
}
