using DocesCabana.Application.Contracts.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DocesCabana.Infrastructure.Services;

public class CacheEmMemoria : ICacheDaAplicacao
{
    private readonly IMemoryCache _memoryCache;

    public CacheEmMemoria(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T> ObterOuCriar<T>(string chave, Func<Task<T>> fabrica, TimeSpan duracao)
    {
        if (_memoryCache.TryGetValue(chave, out T? existente) && existente is not null)
            return existente;

        var valor = await fabrica();
        _memoryCache.Set(chave, valor, duracao);
        return valor;
    }

    public void Remover(string chave) => _memoryCache.Remove(chave);
}
