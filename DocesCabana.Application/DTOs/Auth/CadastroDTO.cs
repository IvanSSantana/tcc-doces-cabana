using System.ComponentModel.DataAnnotations;

namespace DocesCabana.Application.DTOs.Auth;

public class CadastroDTO
{
    [Required(ErrorMessage ="Nome é obrigatório!")]
    [MaxLength(100, ErrorMessage ="O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
 
    [Required(ErrorMessage ="O E-mail é obrigatório")]
    [EmailAddress(ErrorMessage ="O E-mail é inválido")]
    [MaxLength(100, ErrorMessage ="O email deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;
}
