using DocesCabana.Application.DTOs.Autenticacao;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class EsqueceuSenhaDTOValidator : AbstractValidator<EsqueceuSenhaDTO>
{
    public EsqueceuSenhaDTOValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O login é obrigatório!")
            .Must(FormatoLoginValidator.ValidarEmailOuCpf).WithMessage("O formato do login deve ser um e-mail ou um CPF válido.");
    }
}
