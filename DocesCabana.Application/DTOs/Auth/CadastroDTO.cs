namespace DocesCabana.Application.DTOs.Auth;

public class CadastroDTO
{
    public string Nome { get; set; } = string.Empty;
 
    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    public string CPF { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    public string ConfirmacaoSenha { get; set; } = string.Empty;
}
