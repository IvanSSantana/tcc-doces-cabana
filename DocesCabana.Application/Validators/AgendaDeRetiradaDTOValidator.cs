using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class AgendaDeRetiradaDTOValidator : AbstractValidator<AgendaDeRetiradaDTO>
{
    public AgendaDeRetiradaDTOValidator()
    {
        RuleFor(x => x.TempoDePreparoEmMinutos)
            .NotNull().WithMessage("Informe o tempo de preparo.")
            .InclusiveBetween(AgendaDeRetirada.PreparoMinimo, AgendaDeRetirada.PreparoMaximo)
                .WithMessage($"O tempo de preparo deve ficar entre {AgendaDeRetirada.PreparoMinimo} e {AgendaDeRetirada.PreparoMaximo} minutos.");

        RuleFor(x => x.InicioDoAlmoco)
            .NotNull().WithMessage("Informe o início do almoço.")
            .When(x => x.TemAlmoco);

        RuleFor(x => x.FimDoAlmoco)
            .NotNull().WithMessage("Informe o fim do almoço.")
            .When(x => x.TemAlmoco);

        RuleFor(x => x.FimDoAlmoco)
            .GreaterThan(x => x.InicioDoAlmoco).WithMessage("O fim do almoço precisa ser depois do início.")
            .When(x => x.TemAlmoco && x.InicioDoAlmoco is not null && x.FimDoAlmoco is not null);

        RuleFor(x => x.Horarios)
            .Must(h => h.Select(d => d.DiaDaSemana).Distinct().Count() == 7 && h.Count == 7)
                .WithMessage("Informe o horário dos sete dias da semana.");

        RuleForEach(x => x.Horarios)
            .SetValidator(new HorarioDeRetiradaDTOValidator());
    }
}
