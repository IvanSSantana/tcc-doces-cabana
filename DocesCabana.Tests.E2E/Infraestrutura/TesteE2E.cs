using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Infraestrutura;

/// <summary>
/// Base de todo teste de fluxo. Um contexto de navegador novo por teste —
/// cookie de um não vaza para o outro (RN-02 da 007).
/// </summary>
[Collection(ColecaoE2E.Nome)]
[Trait("Categoria", "E2E")]
public abstract class TesteE2E : IAsyncLifetime
{
    private readonly FixtureE2E _fixture;
    private IBrowserContext _contexto = null!;

    protected IPage Pagina { get; private set; } = null!;
    protected AplicacaoEmExecucao Aplicacao => _fixture.Aplicacao;
    protected string UrlBase => Aplicacao.UrlBase;

    protected TesteE2E(FixtureE2E fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _contexto = await _fixture.Navegador.NewContextAsync();
        await _contexto.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        });
        Pagina = await _contexto.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _contexto.CloseAsync();
    }

    /// <summary>
    /// Envolve o corpo de cada teste para gravar o rastro do Playwright em
    /// <c>rastros-e2e/</c> quando ele falha (RF-08). O xUnit v2 não expõe o
    /// resultado do teste em <see cref="IAsyncLifetime.DisposeAsync"/> — sem
    /// um adaptador de framework (que a constituição não permite: seria um
    /// segundo runner), a captura só é alcançável envolvendo o corpo aqui.
    /// </summary>
    protected async Task Executar(Func<Task> corpo, [CallerMemberName] string nomeDoTeste = "")
    {
        try
        {
            await corpo();
        }
        catch
        {
            var pastaDeRastros = Path.Combine(AppContext.BaseDirectory, "rastros-e2e");
            Directory.CreateDirectory(pastaDeRastros);
            var caminho = Path.Combine(pastaDeRastros, $"{nomeDoTeste}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.zip");

            await _contexto.Tracing.StopAsync(new TracingStopOptions { Path = caminho });
            throw;
        }
    }
}
