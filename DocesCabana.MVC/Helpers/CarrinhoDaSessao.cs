using System.Text.Json;
using DocesCabana.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace DocesCabana.MVC.Helpers;

// Leitura e escrita do carrinho avulso (visitante) na sessão — spec 017,
// plano §5. Só isso: nenhuma regra de negócio mora aqui, todas vivem em
// CarrinhoService (as operações avulsas aplicam as mesmas regras das
// persistidas, só que sobre esta lista em vez do banco).
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

    // Usado pelo filtro de fusão (Fase 7): depois de juntar ao carrinho do
    // banco, o avulso não pode sobrar (CA-14).
    public static void Limpar(this ISession sessao) => sessao.Remove(Chave);
}
