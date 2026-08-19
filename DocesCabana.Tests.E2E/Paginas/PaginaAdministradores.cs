using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

/// <summary>Cobre as duas telas da gestão de administradores: lista e cadastro.</summary>
public class PaginaAdministradores
{
    private readonly IPage _pagina;
    private ILocator ContainerIndice => _pagina.Locator(".container-administradores");
    private ILocator ContainerCadastro => _pagina.Locator(".container-autenticacao");
    private ILocator FormularioCadastro => ContainerCadastro.Locator("form.formulario-autenticacao");

    public PaginaAdministradores(IPage pagina) => _pagina = pagina;

    public async Task AbrirIndice(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Admin/Administrador");

    public async Task AbrirCadastro(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Admin/Administrador/Cadastro");

    public async Task IrParaCadastro() =>
        await ContainerIndice.GetByRole(AriaRole.Link, new() { Name = "Novo administrador" }).ClickAsync();

    public ILocator LinhaComEmail(string email) => ContainerIndice.Locator("table tbody tr", new() { HasText = email });
    public ILocator MensagemDeConfirmacao => ContainerIndice.Locator(".resumo-sucesso .mensagem-sucesso");

    public async Task PreencherCadastro(DadosDeCadastro dados)
    {
        await ContainerCadastro.GetByLabel("Nome Completo").FillAsync(dados.Nome);
        await ContainerCadastro.GetByLabel("E-mail").FillAsync(dados.Email);
        await ContainerCadastro.GetByLabel("Número de celular").FillAsync(dados.Celular);
        await ContainerCadastro.GetByLabel("Data de Nascimento").FillAsync(dados.DataNascimento);
        await ContainerCadastro.GetByLabel("CPF").FillAsync(dados.Cpf);
        await ContainerCadastro.Locator("#input-senha-cadastro").FillAsync(dados.Senha);
        await ContainerCadastro.Locator("#input-confirmacao-senha").FillAsync(dados.Senha);
    }

    public async Task EnviarCadastro() =>
        await ContainerCadastro.GetByRole(AriaRole.Button, new() { Name = "Cadastrar Administrador" }).ClickAsync();

    // Mesma colisão de ".resumo-erros" da tela de cadastro de cliente — ver
    // PaginaCadastro.MensagemDeErroGeral.
    public ILocator MensagemDeErroCadastro => FormularioCadastro.Locator("> div.resumo-erros .mensagem-erro");
}
