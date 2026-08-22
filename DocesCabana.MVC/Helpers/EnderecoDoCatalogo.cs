using DocesCabana.Application.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Helpers;

// Único lugar que monta endereço de catálogo (spec 016, plano §4) — categoria,
// subcategorias, sem açúcar, ordenação, termo de busca e página, todos por
// apelido/valor legível, nunca por identificador. Usado pela barra lateral,
// pela paginação e pelo cabeçalho, para que só exista uma regra de
// serialização de endereço no lado do servidor (o irmão desta regra, do lado
// do cliente, é catalogo.js, que serializa o próprio formulário).
public static class EnderecoDoCatalogo
{
    public static string Montar(
        IUrlHelper urlHelper,
        string? apelidoDaCategoria,
        IEnumerable<string>? apelidosDeSubcategoria = null,
        bool apenasSemAcucar = false,
        OrdenacaoCatalogo ordenacao = OrdenacaoCatalogo.MelhorAvaliados,
        string? termo = null,
        int? pagina = null)
    {
        var caminhoBase = apelidoDaCategoria is null
            ? urlHelper.Action("Index", "Catalogo")!
            : urlHelper.Action("Index", "Catalogo", new { apelido = apelidoDaCategoria })!;

        var partesDaConsulta = new List<string> { $"ordenacao={ordenacao}" };

        if (pagina.HasValue)
            partesDaConsulta.Add($"pagina={pagina.Value}");

        if (apenasSemAcucar)
            partesDaConsulta.Add("semAcucar=true");

        if (apelidosDeSubcategoria is not null)
        {
            foreach (var apelido in apelidosDeSubcategoria)
                partesDaConsulta.Add($"subcategorias={Uri.EscapeDataString(apelido)}");
        }

        if (!string.IsNullOrWhiteSpace(termo))
            partesDaConsulta.Add($"termo={Uri.EscapeDataString(termo)}");

        return $"{caminhoBase}?{string.Join("&", partesDaConsulta)}";
    }
}
