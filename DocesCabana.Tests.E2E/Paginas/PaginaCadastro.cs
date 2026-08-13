using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

/// <summary>Dados de um cadastro válido, prontos para preencher o formulário.</summary>
public record DadosDeCadastro(
    string Nome,
    string Email,
    string Celular,
    string DataNascimento,
    string Cpf,
    string Senha);

/// <summary>
/// Cadastro de cliente (<c>/Autenticacao/Cadastro</c>). O de administrador
/// tem o mesmo formulário — ver <see cref="PaginaAdministradores"/>.
/// </summary>
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
        // Celular, CPF e Data de Nascimento têm máscara em JavaScript que
        // formata a partir dos dígitos digitados — basta preencher o valor
        // cru (autenticacao.js, formatarTelefone/formatarCPF/formatarDataNascimento).
        await Container.GetByLabel("Número de celular").FillAsync(dados.Celular);
        await Container.GetByLabel("Data de Nascimento").FillAsync(dados.DataNascimento);
        await Container.GetByLabel("CPF").FillAsync(dados.Cpf);
        await PreencherSenha(dados.Senha);
    }

    public async Task PreencherSenha(string senha)
    {
        // Mesma quebra de associação label/input do Login — ver PaginaLogin.
        await Container.Locator("#input-senha-cadastro").FillAsync(senha);
        await Container.Locator("#input-confirmacao-senha").FillAsync(senha);
    }

    public async Task Enviar() =>
        await Container.GetByRole(AriaRole.Button, new() { Name = "Cadastrar" }).ClickAsync();

    // ".resumo-erros" se repete três vezes na página: a dica fixa de senha
    // (#requisitos-senha, sempre no DOM, só escondida por CSS), o erro da
    // própria senha, e o erro geral do formulário. "> div.resumo-erros"
    // (filho direto do <form>) isola o terceiro — os outros dois vivem
    // dentro de ".campo-entrada", um nível a mais.
    public ILocator MensagemDeErroGeral => Formulario.Locator("> div.resumo-erros .mensagem-erro");

    // O campo Senha tem dois ".resumo-erros": a dica fixa (com id, excluída
    // aqui) e o bloco de erro do servidor. CA-03/CA-06 verificam o segundo.
    public ILocator ErroDeSenha => Container.Locator("#input-senha-cadastro")
        .Locator("xpath=ancestor::div[contains(@class,'campo-entrada')]")
        .Locator("> div.resumo-erros:not(#requisitos-senha)");
}
