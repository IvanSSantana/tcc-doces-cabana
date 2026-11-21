using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class AplicacaoDeCupomDTOValidator : AbstractValidator<AplicacaoDeCupomDTO>
{
    public AplicacaoDeCupomDTOValidator()
    {
        RuleFor(x => x.Codigo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o código do cupom.")
            .Must(c => c.Trim().Length <= Cupom.TamanhoMaximoDoCodigo)
                .WithMessage("O código do cupom tem no máximo 20 caracteres.");
    }
}
