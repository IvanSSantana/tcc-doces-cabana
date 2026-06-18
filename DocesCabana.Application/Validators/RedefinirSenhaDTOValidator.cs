using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class RedefinirSenhaDTOValidator : AbstractValidator<RedefinirSenhaDTO>
{
    public RedefinirSenhaDTOValidator()
    {
        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória!")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
            .MaximumLength(50).WithMessage("A senha deve ter no máximo 50 caracteres.")
            .Matches(@"[a-z]").WithMessage("A senha deve conter pelo menos uma letra minúscula.")
            .Matches(@"[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
            .Matches(@"\d").WithMessage("A senha deve conter pelo menos um número.")
            .Matches(@"[\W_]").WithMessage("A senha deve conter pelo menos um caractere especial.");

        RuleFor(x => x.ConfirmacaoSenha)
            .NotEmpty().WithMessage("A confirmação da senha é obrigatória!")
            .Equal(x => x.Senha).WithMessage("As senhas não coincidem.")
            .MaximumLength(50).WithMessage("A confirmação da senha deve ter no máximo 50 caracteres.");
    }
}
