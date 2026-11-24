using DocesCabana.Application.Contracts.Services;

namespace DocesCabana.Tests.Units.Services;

/// <summary>
/// Cache de verdade em dicionário, sem expiração: prova o comportamento (segunda leitura
/// não chama a fábrica; remoção força nova leitura) sem depender de Moq para cache.
/// </summary>
internal sealed class CacheFalso : ICacheDaAplicacao
{
    private readonly Dictionary<string, object?> _valores = new();

    public List<string> ChavesRemovidas { get; } = [];

    public async Task<T> ObterOuCriar<T>(string chave, Func<Task<T>> fabrica, TimeSpan duracao)
    {
        if (_valores.TryGetValue(chave, out var existente))
            return (T)existente!;

        var valor = await fabrica();
        _valores[chave] = valor;
        return valor;
    }

    public void Remover(string chave)
    {
        ChavesRemovidas.Add(chave);
        _valores.Remove(chave);
    }
}
