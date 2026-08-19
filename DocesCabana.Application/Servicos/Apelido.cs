using System.Globalization;
using System.Text;

namespace DocesCabana.Application.Servicos;

// Deriva o apelido de categoria usado no endereço (spec 012, RF-02) a partir
// do nome, sem guardar nada: "Empório" -> "emporio". O casamento acontece em
// memória, sobre as poucas categorias que a tela já carrega — não há
// coluna nem consulta dedicada a este fim (plano §8, alternativa descartada).
public static class Apelido
{
    public static string De(string nome)
    {
        var semAcento = RemoverAcentos(nome.Trim().ToLowerInvariant());
        var comHifens = semAcento.Replace(' ', '-').Replace('/', '-');

        while (comHifens.Contains("--"))
            comHifens = comHifens.Replace("--", "-");

        return comHifens.Trim('-');
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
