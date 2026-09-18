using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Servicos;

public static class Apelido
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
