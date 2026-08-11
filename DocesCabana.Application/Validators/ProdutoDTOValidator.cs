using DocesCabana.Application.DTOs;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class ProdutoDTOValidator : AbstractValidator<ProdutoDTO>
{
    public ProdutoDTOValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório!")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleFor(x => x.ImagemUrl)
            .NotEmpty().WithMessage("Imagem é obrigatória!")
            .Must(SerUrlAbsolutaHttp).WithMessage("URL da imagem inválida.");

        RuleFor(x => x.SubcategoriaId)
            .NotEqual(Guid.Empty).WithMessage("Subcategoria inválida.");
    }

    private static bool SerUrlAbsolutaHttp(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
