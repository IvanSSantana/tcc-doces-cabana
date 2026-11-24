using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class BannerDTOValidator : AbstractValidator<BannerDTO>
{
    public BannerDTOValidator()
    {
        RuleFor(x => x.TextoAlternativo)
            .Must(texto => !string.IsNullOrWhiteSpace(texto)).WithMessage("Descreva a imagem do banner.")
            .MaximumLength(Banner.TamanhoMaximoDoTextoAlternativo)
            .WithMessage($"A descrição da imagem deve ter no máximo {Banner.TamanhoMaximoDoTextoAlternativo} caracteres.");

        RuleFor(x => x.Ordem)
            .NotNull().WithMessage("Informe a ordem do banner.")
            .GreaterThanOrEqualTo((short)0).WithMessage("A ordem não pode ser negativa.");

        RuleFor(x => x.DataInicio)
            .NotNull().WithMessage("Informe a data de início.");

        RuleFor(x => x.DataFim)
            .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data de fim não pode ser anterior à de início.")
            .When(x => x.DataInicio is not null && x.DataFim is not null);

        RuleFor(x => x.TipoDeDestino)
            .IsInEnum().WithMessage("Destino inválido.");

        RuleFor(x => x.CategoriaId)
            .NotNull().WithMessage("Escolha a categoria.")
            .NotEqual(Guid.Empty).WithMessage("Escolha a categoria.")
            .When(x => x.TipoDeDestino == TipoDeDestinoDoBanner.Categoria);

        RuleFor(x => x.SubcategoriaId)
            .NotNull().WithMessage("Escolha a subcategoria.")
            .NotEqual(Guid.Empty).WithMessage("Escolha a subcategoria.")
            .When(x => x.TipoDeDestino == TipoDeDestinoDoBanner.Subcategoria);

        RuleFor(x => x.ProdutoId)
            .NotNull().WithMessage("Escolha o produto.")
            .NotEqual(Guid.Empty).WithMessage("Escolha o produto.")
            .When(x => x.TipoDeDestino == TipoDeDestinoDoBanner.Produto);
    }
}
