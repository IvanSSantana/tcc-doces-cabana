using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaLogin
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".container-autenticacao");

    public PaginaLogin(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Autenticacao/Login");

    public async Task Entrar(string login, string senha)
    {
        await Container.GetByLabel("E-mail ou CPF").FillAsync(login);
        // O <label asp-for="Senha"> gera for="Senha", mas o input tem id
        // explícito "input-senha" — GetByLabel não associa os dois.
        await Container.Locator("#input-senha").FillAsync(senha);
        await Container.GetByRole(AriaRole.Button, new() { Name = "Entrar" }).ClickAsync();
    }

    public ILocator MensagemDeErro => Container.Locator(".resumo-erros .mensagem-erro");
    public ILocator LinkEsqueceuSenha => Container.GetByRole(AriaRole.Link, new() { Name = "Esqueceu a senha?" });
    public ILocator LinkCriarConta => Container.GetByRole(AriaRole.Link, new() { Name = "Criar uma conta" });
}
