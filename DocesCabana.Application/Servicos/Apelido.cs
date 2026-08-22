using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Servicos;

// Deriva o apelido de categoria/subcategoria usado no endereço (spec 012,
// RF-02; spec 016, RN-03) a partir do nome, sem guardar nada: "Empório" ->
// "emporio". O casamento acontece em memória, sobre as poucas categorias que
// a tela já carrega — não há coluna nem consulta dedicada a este fim (spec
// 012, plano §8, alternativa descartada). A remoção de acento e caixa é
// TextoHelper (Domain, spec 016) — este arquivo só acrescenta os hifens.
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
