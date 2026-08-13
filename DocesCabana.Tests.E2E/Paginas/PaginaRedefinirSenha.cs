using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaRedefinirSenha
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".container-autenticacao");

    public PaginaRedefinirSenha(IPage pagina) => _pagina = pagina;

    /// <summary>Abre o link exatamente como recebido — vem completo por e-mail (CaixaDeEntrada).</summary>
    public async Task AbrirPeloLink(string link) => await _pagina.GotoAsync(link);

    public async Task DefinirNovaSenha(string senha)
    {
        // Mesma quebra de associação label/input das outras telas de senha.
        await Container.Locator("#input-senha-cadastro").FillAsync(senha);
        await Container.Locator("#input-confirmacao-senha").FillAsync(senha);
        await Container.GetByRole(AriaRole.Button, new() { Name = "Confirmar" }).ClickAsync();
    }
}
