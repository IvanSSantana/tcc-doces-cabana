using System.ComponentModel.DataAnnotations;

namespace DocesCabana.Application.DTOs.Auth;

public class CadastroDTO
{
    [Required(ErrorMessage ="Nome é obrigatório!")]
    [MaxLength(100, ErrorMessage ="O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
 
    [Required(ErrorMessage ="O E-mail é obrigatório")]
    [EmailAddress(ErrorMessage ="O E-mail é inválido")]
    [MaxLength(100, ErrorMessage ="O E-mail deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage ="O número de telefone é obrigatório!")]
    [Phone(ErrorMessage ="O número de telefone é inválido")]
    [MaxLength(20, ErrorMessage ="O número de telefone deve ter no máximo 20 caracteres")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage ="A data de nascimento é obrigatória!")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage ="O CPF é obrigatório!")]
    [MaxLength(14, ErrorMessage ="O CPF deve ter no máximo 14 caracteres")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage ="A senha é obrigatória!")]
    [MinLength(6, ErrorMessage ="A senha deve ter no mínimo 6 caracteres")]
    [MaxLength(50, ErrorMessage ="A senha deve ter no máximo 50 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage ="A confirmação da senha é obrigatória!")]
    [Compare(nameof(Senha), ErrorMessage ="As senhas não coincidem!")]

    public string ConfirmacaoSenha { get; set; } = string.Empty;


}
