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

        // RN-01: descrição é opcional, por isso sem NotEmpty — só o limite.
        RuleFor(x => x.Descricao)
            .MaximumLength(4000).WithMessage("Descrição deve ter no máximo 4000 caracteres.");

        // RF-02/RN-01 (spec 020): produto sem medida não é despachável.
        RuleFor(x => x.Peso)
            .GreaterThan(0).WithMessage("Peso deve ser maior que zero.");

        RuleFor(x => x.Altura)
            .GreaterThan(0).WithMessage("Altura deve ser maior que zero.");

        RuleFor(x => x.Largura)
            .GreaterThan(0).WithMessage("Largura deve ser maior que zero.");

        RuleFor(x => x.Comprimento)
            .GreaterThan(0).WithMessage("Comprimento deve ser maior que zero.");
    }

    private static bool SerUrlAbsolutaHttp(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
