using DocesCabana.Application.DTOs;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class ImagemParaEnvioDTOValidator : AbstractValidator<ImagemParaEnvioDTO>
{
    // Regra de negócio ("o que a loja aceita publicar"), não configuração —
    // fica testável sem subir configuração nenhuma (spec 027, plano §4).
    private static readonly string[] ExtensoesAceitas = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] ContentTypesAceitos = ["image/jpeg", "image/png", "image/webp"];
    private const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;

    public ImagemParaEnvioDTOValidator()
    {
        RuleFor(x => x.NomeDoArquivo)
            .Must(TerExtensaoAceita).WithMessage("Formato de imagem não aceito. Use JPG, PNG ou WEBP.");

        RuleFor(x => x.ContentType)
            .Must(ct => ContentTypesAceitos.Contains(ct)).WithMessage("Formato de imagem não aceito. Use JPG, PNG ou WEBP.");

        RuleFor(x => x.TamanhoEmBytes)
            .LessThanOrEqualTo(TamanhoMaximoEmBytes).WithMessage("Imagem acima do tamanho máximo de 5 MB.");
    }

    private static bool TerExtensaoAceita(string nomeDoArquivo) =>
        ExtensoesAceitas.Contains(Path.GetExtension(nomeDoArquivo).ToLowerInvariant());
}
