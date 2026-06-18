using System.ComponentModel.DataAnnotations;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class EsqueceuSenhaDTOValidator : AbstractValidator<EsqueceuSenhaDTO>
{
    public EsqueceuSenhaDTOValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O login é obrigatório!")
            .Must(ValidarEmailOuCpf).WithMessage("O formato do login deve ser um e-mail ou um CPF válido.");
    }

    private bool ValidarEmailOuCpf(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        var validadorEmail = new EmailAddressAttribute();

        bool ehEmail = validadorEmail.IsValid(login);
        bool ehCpf = CpfHelper.CpfValido(login);

        return ehCpf || ehEmail;
    }
}
