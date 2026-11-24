using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace DocesCabana.Infrastructure.Services;

/// <summary>
/// Embrulha a cotação da Melhor Envio: toda ação do carrinho com CEP na sessão cotava de novo,
/// mesmo sem o carrinho mudar. A chave é o CEP (só dígitos) mais produto, quantidade e preço de
/// cada linha, em ordem fixa — carrinho diferente nunca recebe o frete de outro. Cotação sem
/// opção (falha ou CEP sem atendimento) não fica guardada, para a próxima tentativa ir à API.
/// </summary>
public class FreteServiceComCache : IFreteService
{
    private static readonly TimeSpan Duracao = TimeSpan.FromMinutes(10);

    private readonly IFreteService _interno;
    private readonly IMemoryCache _memoryCache;

    public FreteServiceComCache(IFreteService interno, IMemoryCache memoryCache)
    {
        _interno = interno;
        _memoryCache = memoryCache;
    }

    public async Task<CotacaoDeFreteDTO> Cotar(string cepDestino, IReadOnlyList<LinhaDoCarrinhoDTO> itensDisponiveis)
    {
        var chave = MontarChave(cepDestino, itensDisponiveis);

        if (_memoryCache.TryGetValue(chave, out CotacaoDeFreteDTO? guardada) && guardada is not null)
            return guardada with { CepConsultado = cepDestino };

        var cotacao = await _interno.Cotar(cepDestino, itensDisponiveis);

        if (cotacao.Opcoes.Count > 0)
            _memoryCache.Set(chave, cotacao, Duracao);

        return cotacao;
    }

    private static string MontarChave(string cep, IEnumerable<LinhaDoCarrinhoDTO> itens)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());
        var linhas = itens
            .OrderBy(i => i.ProdutoId)
            .Select(i => $"{i.ProdutoId:N}x{i.Quantidade}@{i.PrecoUnitario}");

        return $"frete:{digitos}:{string.Join(",", linhas)}";
    }
}
