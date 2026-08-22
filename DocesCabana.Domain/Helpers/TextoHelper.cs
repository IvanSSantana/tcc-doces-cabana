using System.Globalization;
using System.Text;

namespace DocesCabana.Domain.Helpers;

// Normalização de texto para comparação — sem acento, sem caixa, sem espaço
// nas pontas (spec 016). Vivia só dentro de Apelido.De (Application); desceu
// para o Domain porque Produto.NomeNormalizado também precisa dela, e uma
// entidade de domínio não pode referenciar a Application (Princípio I). Só
// BCL, como CepHelper e CpfHelper.
public static class TextoHelper
{
    public static string Normalizar(string texto)
    {
        var semAcento = RemoverAcentos(texto.Trim().ToLowerInvariant());

        while (semAcento.Contains("  "))
            semAcento = semAcento.Replace("  ", " ");

        return semAcento;
    }

    private static string RemoverAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var construtor = new StringBuilder();

        foreach (var caractere in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                construtor.Append(caractere);
        }

        return construtor.ToString().Normalize(NormalizationForm.FormC);
    }
}
