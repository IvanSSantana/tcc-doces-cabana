using System.ComponentModel.DataAnnotations;

namespace DocesCabana.Application.DTOs.Auth;

public class LoginDTO
{
    [Required(ErrorMessage = "E-mail ou telefone é obrigatório!")]
    [MaxLength(100, ErrorMessage ="Login deve ter no máximo 100 caracteres")]
    public string Login { get; set; } = string.Empty; //string.Empty = "" --> texto vazio

    [Required(ErrorMessage ="A senha é obrigatória!")]
    [MinLength(6, ErrorMessage ="A senha deve ter no mínimo 6 caracteres")]
    [MaxLength(50, ErrorMessage ="A senha deve ter no máximo 50 caracteres")]
    public string Senha { get; set; } = string.Empty;
}
