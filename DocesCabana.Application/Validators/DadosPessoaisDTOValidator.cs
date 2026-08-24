using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

// Reaproveita as regras de Nome, Celular e DataNascimento de
// CadastroDTOValidator (plano §9, risco 7) — mesma validação, mesma
// mensagem, para que o mesmo dado inválido não passe no cadastro e falhe na
// conta, ou o contrário. CPF não tem regra aqui: não é campo de formulário
// (RN-06), viaja no DTO só para exibição.
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
    }
}
