using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Helpers;

public static class ApelidoHelper
{
    public static string De(string nome)
    {
        var normalizado = TextoHelper.Normalizar(nome);
        var comHifens = normalizado.Replace(' ', '-').Replace('/', '-');

        while (comHifens.Contains("--"))
            comHifens = comHifens.Replace("--", "-");

        return comHifens.Trim('-');
    }
}
