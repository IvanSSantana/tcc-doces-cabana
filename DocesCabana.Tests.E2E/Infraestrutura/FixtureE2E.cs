using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Infraestrutura;

public sealed class FixtureE2E : IAsyncLifetime
{
    public AplicacaoEmExecucao Aplicacao { get; private set; } = null!;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Navegador { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Aplicacao = await AplicacaoEmExecucao.Subir();
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Navegador = await Playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        await Navegador.CloseAsync();
        Playwright.Dispose();
        await Aplicacao.DisposeAsync();
    }
}
