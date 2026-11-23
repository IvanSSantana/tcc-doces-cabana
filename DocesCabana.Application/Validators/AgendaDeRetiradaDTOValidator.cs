using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class AgendaDeRetiradaDTOValidator : AbstractValidator<AgendaDeRetiradaDTO>
{
    public AgendaDeRetiradaDTOValidator()
    {
        RuleFor(x => x.DuracaoDaFaixaEmMinutos)
            .NotNull().WithMessage("Informe a duração da faixa.")
            .InclusiveBetween(AgendaDeRetirada.DuracaoMinima, AgendaDeRetirada.DuracaoMaxima)
                .WithMessage($"A duração da faixa deve ficar entre {AgendaDeRetirada.DuracaoMinima} e {AgendaDeRetirada.DuracaoMaxima} minutos.");

        RuleFor(x => x.PreparoEmMinutos)
            .NotNull().WithMessage("Informe o tempo de preparo.")
            .InclusiveBetween(0, AgendaDeRetirada.PreparoMaximo)
                .WithMessage($"O tempo de preparo deve ficar entre 0 e {AgendaDeRetirada.PreparoMaximo} minutos.");

        RuleFor(x => x.Horarios)
            .Must(h => h.Select(d => d.DiaDaSemana).Distinct().Count() == 7 && h.Count == 7)
                .WithMessage("Informe o horário dos sete dias da semana.");

        RuleForEach(x => x.Horarios)
            .SetValidator(new HorarioDeRetiradaDTOValidator());
    }
}
