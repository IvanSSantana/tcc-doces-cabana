using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public record DadosDeCadastro(
    string Nome,
    string Email,
    string Celular,
    string DataNascimento,
    string Cpf,
    string Senha);

public class PaginaCadastro
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".container-autenticacao");
    private ILocator Formulario => Container.Locator("form.formulario-autenticacao");

    public PaginaCadastro(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Autenticacao/Cadastro");

    public async Task Preencher(DadosDeCadastro dados)
    {
        await Container.GetByLabel("Nome Completo").FillAsync(dados.Nome);
        await Container.GetByLabel("E-mail").FillAsync(dados.Email);
        await Container.GetByLabel("Número de celular").FillAsync(dados.Celular);
        await Container.GetByLabel("Data de Nascimento").FillAsync(dados.DataNascimento);
        await Container.GetByLabel("CPF").FillAsync(dados.Cpf);
        await PreencherSenha(dados.Senha);
    }

    public async Task PreencherSenha(string senha)
    {
        await Container.Locator("#input-senha-cadastro").FillAsync(senha);
        await Container.Locator("#input-confirmacao-senha").FillAsync(senha);
    }

    public async Task Enviar() =>
        await Container.GetByRole(AriaRole.Button, new() { Name = "Cadastrar" }).ClickAsync();

    public ILocator MensagemDeErroGeral => Formulario.Locator("> div.resumo-erros .mensagem-erro");

    public ILocator ErroDeSenha => Container.Locator("#input-senha-cadastro")
        .Locator("xpath=ancestor::div[contains(@class,'campo-entrada')]")
        .Locator("> div.resumo-erros:not(#requisitos-senha)");
}
