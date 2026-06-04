namespace DocesCabana.Application.DTOs.Autenticacao;

public class CadastroDTO
{
    public string? Nome { get; set; }
 
    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public DateTime? DataNascimento { get; set; }

    public string? CPF { get; set; } 

    public string? Senha { get; set; } 

    public string? ConfirmacaoSenha { get; set; } 
}
