using DocesCabana.Application.DTOs;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class NovaAvaliacaoDTOValidator : AbstractValidator<NovaAvaliacaoDTO>
{
    public NovaAvaliacaoDTOValidator()
    {
        RuleFor(x => x.ProdutoId)
            .NotEmpty().WithMessage("Produto inválido.");

        RuleFor(x => x.Nota)
            .NotNull().WithMessage("Escolha uma nota de 1 a 5.")
            .InclusiveBetween((byte)1, (byte)5).WithMessage("Escolha uma nota de 1 a 5.");

        RuleFor(x => x.Comentario)
            .MaximumLength(255).WithMessage("O comentário deve ter no máximo 255 caracteres.");
    }
}
