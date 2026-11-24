using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DocesCabana.Tests.Units.Services;

public class CacheEmMemoriaTests
{
    private readonly CacheEmMemoria _cache = new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task Dado_ChaveJaLida_Quando_ObterOuCriarDeNovo_Entao_NaoDeveChamarAFabrica()
    {
        var chamadas = 0;
        Task<string> Fabrica() { chamadas++; return Task.FromResult("valor"); }

        await _cache.ObterOuCriar("chave", Fabrica, TimeSpan.FromMinutes(1));
        var segunda = await _cache.ObterOuCriar("chave", Fabrica, TimeSpan.FromMinutes(1));

        Assert.Equal("valor", segunda);
        Assert.Equal(1, chamadas);
    }

    [Fact]
    public async Task Dado_ChaveRemovida_Quando_ObterOuCriar_Entao_DeveChamarAFabricaDeNovo()
    {
        var chamadas = 0;
        Task<int> Fabrica() => Task.FromResult(++chamadas);

        await _cache.ObterOuCriar("chave", Fabrica, TimeSpan.FromMinutes(1));
        _cache.Remover("chave");
        var depois = await _cache.ObterOuCriar("chave", Fabrica, TimeSpan.FromMinutes(1));

        Assert.Equal(2, depois);
    }
}
