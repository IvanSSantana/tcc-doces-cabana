using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Enums;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class PromocaoDTOValidator : AbstractValidator<PromocaoDTO>
{
    public PromocaoDTOValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório!")
            .MaximumLength(255).WithMessage("Nome deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(255).WithMessage("Descrição deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de desconto inválido.")
            .NotEqual(TipoDeDesconto.FreteGratis).WithMessage("Frete grátis só existe como cupom.");

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

        RuleFor(x => x.Abrangencia)
            .IsInEnum().WithMessage("Abrangência inválida.");

        RuleFor(x => x.CategoriaId)
            .NotNull().WithMessage("Escolha a categoria.")
            .NotEqual(Guid.Empty).WithMessage("Escolha a categoria.")
            .When(x => x.Abrangencia == AbrangenciaDaPromocao.Categoria);

        RuleFor(x => x.SubcategoriaId)
            .NotNull().WithMessage("Escolha a subcategoria.")
            .NotEqual(Guid.Empty).WithMessage("Escolha a subcategoria.")
            .When(x => x.Abrangencia == AbrangenciaDaPromocao.Subcategoria);

        RuleFor(x => x.ProdutoIds)
            .NotEmpty().WithMessage("Escolha ao menos um produto.")
            .When(x => x.Abrangencia == AbrangenciaDaPromocao.ProdutosEscolhidos);
    }
}
