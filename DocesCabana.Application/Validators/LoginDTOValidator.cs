using System.ComponentModel.DataAnnotations;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O Login é obrigatório!")
            .MaximumLength(100).WithMessage("O login deve ter no máximo 100 caracteres.")
            .Must(ValidarEmailOuCpf).WithMessage("O formato do login deve ser um e-mail ou um CPF válido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória!")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
            .MaximumLength(50).WithMessage("A senha deve ter no máximo 50 caracteres.");
    }

    private bool ValidarEmailOuCpf(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        var validadorEmail = new EmailAddressAttribute();

        bool ehEmail = validadorEmail.IsValid(login);
        bool ehCpf = CpfHelper.LoginValido(login);

        return ehCpf || ehEmail;
    }
}
