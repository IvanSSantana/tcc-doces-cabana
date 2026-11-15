using Microsoft.AspNetCore.Http;

namespace DocesCabana.Tests.Units.Controllers;

internal sealed class SessaoFalsa : ISession
{
    private readonly Dictionary<string, byte[]> _valores = new();

    public bool IsAvailable => true;
    public string Id => "sessao-de-teste";
    public IEnumerable<string> Keys => _valores.Keys;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
    public void Set(string key, byte[] value) => _valores[key] = value;
    public void Remove(string key) => _valores.Remove(key);
    public void Clear() => _valores.Clear();
}
