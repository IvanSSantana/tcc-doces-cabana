using FluentValidation;
using DocesCabana.Application.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace DocesCabana.Application.Validators;

public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O campo Login é obrigatório!")
            .MaximumLength(100).WithMessage("O Login deve ter no máximo 100 caracteres")
            .Must(ValidarEmailOuTelefone).WithMessage("O formato do login deve ser um e-mail ou um telefone válido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("O campo Senha é obrigatório!")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres")
            .MaximumLength(50).WithMessage("A senha deve ter no máximo 50 caracteres");
    }

    private bool ValidarEmailOuTelefone(string login)
    {
        if (string.IsNullOrWhiteSpace(login)) 
            return false;

        // Validadores oficiais do .NET
        var validadorEmail = new EmailAddressAttribute();
        var validadorTelefone = new PhoneAttribute();

        bool ehEmail = validadorEmail.IsValid(login);
        bool ehTelefone = validadorTelefone.IsValid(login);

        return ehEmail || ehTelefone;
    }
}