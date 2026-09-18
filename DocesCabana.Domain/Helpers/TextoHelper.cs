using System.Globalization;
using System.Text;

namespace DocesCabana.Domain.Helpers;

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
