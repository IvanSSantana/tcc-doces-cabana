using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class CadastroDTOValidator : AbstractValidator<CadastroDTO>
{
    public CadastroDTOValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório!")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O E-mail é obrigatório!")
            .EmailAddress().WithMessage("O E-mail é inválido.")
            .MaximumLength(100).WithMessage("O E-mail deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("O número de telefone é obrigatório!")
            .MaximumLength(20).WithMessage("O número de telefone deve ter no máximo 20 caracteres.")
            .Must(TelefoneHelper.CelularValido)
            .WithMessage("Número de telefone inválido.");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória!");

        RuleFor(x => x.CPF)
            .NotEmpty().WithMessage("O CPF é obrigatório!")
            .MaximumLength(14).WithMessage("O CPF deve ter no máximo 14 caracteres.")
            .Must(CpfHelper.FormatoValido).WithMessage("O CPF deve conter 11 dígitos.")
            .Must(CpfHelper.CpfValido).WithMessage("CPF inválido.");

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
