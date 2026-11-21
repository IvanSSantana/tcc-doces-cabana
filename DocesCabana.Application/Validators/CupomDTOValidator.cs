using System.Text.RegularExpressions;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public partial class CupomDTOValidator : AbstractValidator<CupomDTO>
{
    public CupomDTOValidator()
    {
        RuleFor(x => x.Codigo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Código é obrigatório!")
            .Must(c => FormatoDoCodigo().IsMatch(Cupom.NormalizarCodigo(c)))
                .WithMessage("O código deve ter de 3 a 20 letras ou números, sem espaços.");

        RuleFor(x => x.Descricao)
            .MaximumLength(255).WithMessage("Descrição deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de desconto inválido.");

        RuleFor(x => x.Valor)
            .NotNull().WithMessage("Informe o percentual.")
            .InclusiveBetween(1, 99).WithMessage("O percentual deve ficar entre 1 e 99.")
            .When(x => x.Tipo == TipoDeDesconto.Percentual);

        RuleFor(x => x.Valor)
            .NotNull().WithMessage("Informe o valor do desconto.")
            .GreaterThan(0).WithMessage("O valor do desconto deve ser maior que zero.")
            .When(x => x.Tipo == TipoDeDesconto.ValorFixo);

        RuleFor(x => x.QuantidadeLeve)
            .NotNull().WithMessage("Informe quantas unidades o cliente leva.")
            .GreaterThanOrEqualTo((short)2).WithMessage("A quantidade de \"leve\" deve ser no mínimo 2.")
            .When(x => x.Tipo == TipoDeDesconto.CompreXLeveY);

        RuleFor(x => x.QuantidadePague)
            .NotNull().WithMessage("Informe quantas unidades o cliente paga.")
            .GreaterThanOrEqualTo((short)1).WithMessage("A quantidade de \"pague\" deve ser no mínimo 1.")
            .When(x => x.Tipo == TipoDeDesconto.CompreXLeveY);

        RuleFor(x => x.QuantidadePague)
            .LessThan(x => x.QuantidadeLeve).WithMessage("A quantidade de \"pague\" deve ser menor que a de \"leve\".")
            .When(x => x.Tipo == TipoDeDesconto.CompreXLeveY
                && x.QuantidadeLeve is not null
                && x.QuantidadePague is not null);

        RuleFor(x => x.DataInicio)
            .NotNull().WithMessage("Informe a data de início.");

        RuleFor(x => x.DataFim)
            .NotNull().WithMessage("Informe a data de fim.");

        RuleFor(x => x.DataFim)
            .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data de fim não pode ser anterior à de início.")
            .When(x => x.DataInicio is not null && x.DataFim is not null);

        RuleFor(x => x.ValorMinimo)
            .GreaterThan(0).WithMessage("O valor mínimo deve ser maior que zero.")
            .When(x => x.ValorMinimo is not null);

        RuleFor(x => x.SubcategoriaId)
            .Null().WithMessage("Restrinja o cupom a uma categoria ou a uma subcategoria, não às duas.")
            .When(x => x.CategoriaId is not null);
    }

    [GeneratedRegex("^[A-Z0-9]{3,20}$")]
    private static partial Regex FormatoDoCodigo();
}
