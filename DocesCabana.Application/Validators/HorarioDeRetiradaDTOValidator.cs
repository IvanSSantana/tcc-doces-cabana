using DocesCabana.Application.DTOs;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class HorarioDeRetiradaDTOValidator : AbstractValidator<HorarioDeRetiradaDTO>
{
    public HorarioDeRetiradaDTOValidator()
    {
        RuleFor(x => x.DiaDaSemana)
            .IsInEnum().WithMessage("Dia da semana inválido.");

        RuleFor(x => x.Abertura)
            .NotNull().WithMessage("Informe a abertura.")
            .When(x => x.Aberto);

        RuleFor(x => x.Fechamento)
            .NotNull().WithMessage("Informe o fechamento.")
            .When(x => x.Aberto);

        RuleFor(x => x.Fechamento)
            .GreaterThan(x => x.Abertura).WithMessage("O fechamento precisa ser depois da abertura.")
            .When(x => x.Aberto && x.Abertura is not null && x.Fechamento is not null);
    }
}
