using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class EnderecoDTOValidator : AbstractValidator<EnderecoDTO>
{
    public EnderecoDTOValidator()
    {
        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório!")
            .MaximumLength(100).WithMessage("O estado deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória!")
            .MaximumLength(150).WithMessage("A cidade deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro é obrigatório!")
            .MaximumLength(255).WithMessage("O bairro deve ter no máximo 255 caracteres.");

        RuleFor(x => x.CEP)
            .NotEmpty().WithMessage("CEP é obrigatório!")
            .Must(CepHelper.FormatoValido).WithMessage("O CEP deve conter 8 dígitos.");

        RuleFor(x => x.Rua)
            .NotEmpty().WithMessage("Rua é obrigatória!")
            .MaximumLength(255).WithMessage("A rua deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Numero)
            .GreaterThan(0).WithMessage("Número deve ser maior que zero.");

        RuleFor(x => x.Complemento)
            .MaximumLength(100).WithMessage("O complemento deve ter no máximo 100 caracteres.");
    }
}
