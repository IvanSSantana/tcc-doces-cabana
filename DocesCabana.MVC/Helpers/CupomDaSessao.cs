using Microsoft.AspNetCore.Http;

namespace DocesCabana.MVC.Helpers;

/// <summary>
/// Código do cupom que o cliente aplicou no carrinho. Guardado na sessão, como o CEP do frete,
/// e reavaliado a cada exibição do carrinho, a cada passo do fechamento e no fechamento.
/// Só o código é guardado, nunca o desconto: o valor é recalculado com o carrinho atual.
/// </summary>
public static class CupomDaSessao
{
    private const string Chave = "CupomDoCarrinho";

    public static string? LerCupom(this ISession sessao)
    {
        var codigo = sessao.GetString(Chave);
        return string.IsNullOrWhiteSpace(codigo) ? null : codigo;
    }

    public static void EscreverCupom(this ISession sessao, string codigo) => sessao.SetString(Chave, codigo);

    public static void LimparCupom(this ISession sessao) => sessao.Remove(Chave);
}
