using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class DadosPessoaisDTOValidator : AbstractValidator<DadosPessoaisDTO>
{
    public DadosPessoaisDTOValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório!")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Celular)
            .NotEmpty().WithMessage("O número de telefone é obrigatório!")
            .MaximumLength(20).WithMessage("O número de telefone deve ter no máximo 20 caracteres.")
            .Must(TelefoneHelper.CelularValido)
            .WithMessage("Número de telefone inválido.");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória!");

        RuleFor(x => x.CPF)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O CPF é obrigatório!")
            .MaximumLength(14).WithMessage("O CPF deve ter no máximo 14 caracteres.")
            .Must(cpf => CpfHelper.FormatoValido(cpf!)).WithMessage("O CPF deve conter 11 dígitos.")
            .Must(cpf => CpfHelper.CpfValido(cpf!)).WithMessage("CPF inválido.");
    }
}
