using System.Text.Json;
using DocesCabana.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace DocesCabana.MVC.Helpers;

public static class CarrinhoDaSessao
{
    private const string Chave = "CarrinhoAvulso";

    public static IReadOnlyList<ItemDoCarrinhoDTO> Ler(this ISession sessao)
    {
        var json = sessao.GetString(Chave);
        if (string.IsNullOrEmpty(json))
            return [];

        return JsonSerializer.Deserialize<List<ItemDoCarrinhoDTO>>(json) ?? [];
    }

    public static void Escrever(this ISession sessao, IReadOnlyList<ItemDoCarrinhoDTO> itens) =>
        sessao.SetString(Chave, JsonSerializer.Serialize(itens));

    public static void Limpar(this ISession sessao) => sessao.Remove(Chave);
}
