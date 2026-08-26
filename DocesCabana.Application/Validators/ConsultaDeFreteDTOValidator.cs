using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Helpers;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class ConsultaDeFreteDTOValidator : AbstractValidator<ConsultaDeFreteDTO>
{
    public ConsultaDeFreteDTOValidator()
    {
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório!")
            .Must(CepHelper.FormatoValido).WithMessage("O CEP deve conter 8 dígitos.");
    }
}
