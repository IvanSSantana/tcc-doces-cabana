using Microsoft.AspNetCore.Http;

namespace DocesCabana.MVC.Helpers;

/// <summary>
/// Último CEP consultado no "Calcular frete" do carrinho. Guardado na sessão para que a
/// estimativa de frete não suma a cada ação do carrinho; apagado quando o pedido é fechado.
/// Só o CEP é guardado, nunca a cotação: preço e disponibilidade são consultados de novo.
/// </summary>
public static class CepDaSessao
{
    private const string Chave = "CepDoFrete";

    public static string? LerCep(this ISession sessao)
    {
        var cep = sessao.GetString(Chave);
        return string.IsNullOrWhiteSpace(cep) ? null : cep;
    }

    public static void EscreverCep(this ISession sessao, string cep) => sessao.SetString(Chave, cep);

    public static void LimparCep(this ISession sessao) => sessao.Remove(Chave);
}
