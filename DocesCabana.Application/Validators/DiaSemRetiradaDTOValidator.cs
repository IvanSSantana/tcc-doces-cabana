using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class DiaSemRetiradaDTOValidator : AbstractValidator<DiaSemRetiradaDTO>
{
    public DiaSemRetiradaDTOValidator(TimeProvider relogio)
    {
        RuleFor(x => x.Data)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Informe a data.")
            .Must(data => data >= HorarioDeBrasilia.Hoje(relogio))
                .WithMessage("Não é possível bloquear uma data que já passou.");

        RuleFor(x => x.Motivo)
            .MaximumLength(DiaSemRetirada.TamanhoMaximoDoMotivo)
                .WithMessage($"O motivo deve ter no máximo {DiaSemRetirada.TamanhoMaximoDoMotivo} caracteres.");
    }
}
