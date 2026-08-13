using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaEsqueceuSenha
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".container-autenticacao");

    public PaginaEsqueceuSenha(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Autenticacao/EsqueceuSenha");

    public async Task Solicitar(string login)
    {
        await Container.GetByLabel("E-mail ou CPF").FillAsync(login);
        await Container.GetByRole(AriaRole.Button, new() { Name = "Solicitar redefinição" }).ClickAsync();
    }

    public ILocator MensagemDeConfirmacao => Container.Locator(".resumo-sucesso .mensagem-sucesso");
}
