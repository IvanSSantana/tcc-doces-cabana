using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

/// <summary>Dados de um endereço válido, prontos para preencher o formulário.</summary>
public record DadosDeEndereco(string CEP, string Estado, string Cidade, string Bairro, string Rua, string Numero, string? Complemento = null);

public class PaginaConta
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-conta");

    public PaginaConta(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Conta");

    public async Task AbrirEnderecos(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Conta/Enderecos");

    public async Task AbrirNovoEndereco(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Conta/NovoEndereco");

    // ── Menu ────────────────────────────────────────────────────────────
    public ILocator MenuDados => Container.Locator(".menu-conta").GetByRole(AriaRole.Link, new() { Name = "Dados pessoais" });
    public ILocator MenuEnderecos => Container.Locator(".menu-conta").GetByRole(AriaRole.Link, new() { Name = "Endereços" });

    // ── Dados pessoais ──────────────────────────────────────────────────
    public ILocator CampoNome => Container.Locator("input[name='Nome']");
    public ILocator CampoCelular => Container.Locator("input[name='Celular']");
    public ILocator CampoDataNascimento => Container.Locator("input[name='DataNascimento']");
    public ILocator CpfSomenteLeitura => Container.Locator(".cpf-somente-leitura");
    public ILocator BotaoSalvarDados => Container.GetByRole(AriaRole.Button, new() { Name = "Salvar alterações" });
    public ILocator ErroDoCelular => Container.Locator("input[name='Celular']")
        .Locator("xpath=ancestor::div[contains(@class,'campo-entrada')]").Locator(".mensagem-erro");

    public async Task PreencherDadosPessoais(string nome, string celular, string dataNascimento)
    {
        await CampoNome.FillAsync(nome);
        await CampoCelular.FillAsync(celular);
        await CampoDataNascimento.FillAsync(dataNascimento);
    }

    // ── Endereços ───────────────────────────────────────────────────────
    public ILocator MensagemVazia => Container.Locator(".enderecos-vazio");
    public ILocator BotaoNovoEndereco => Container.GetByRole(AriaRole.Link, new() { Name = "Novo endereço" });
    public ILocator Cartoes => Container.Locator(".cartao-endereco");
    public ILocator CartaoPrincipal => Container.Locator(".cartao-endereco--principal");

    public ILocator BotaoTornarPrincipal(ILocator cartao) => cartao.GetByRole(AriaRole.Button, new() { Name = "Tornar principal" });
    public ILocator BotaoEditar(ILocator cartao) => cartao.GetByRole(AriaRole.Link, new() { Name = "Editar" });
    public ILocator BotaoExcluir(ILocator cartao) => cartao.GetByRole(AriaRole.Button, new() { Name = "Excluir" });

    // ── Formulário de endereço (cadastro e edição) ─────────────────────
    public ILocator CampoCep => _pagina.Locator("input[name='CEP']");
    public ILocator CampoEstado => _pagina.Locator("input[name='Estado']");
    public ILocator CampoCidade => _pagina.Locator("input[name='Cidade']");
    public ILocator CampoBairro => _pagina.Locator("input[name='Bairro']");
    public ILocator CampoRua => _pagina.Locator("input[name='Rua']");
    public ILocator CampoNumero => _pagina.Locator("input[name='Numero']");
    public ILocator CampoComplemento => _pagina.Locator("input[name='Complemento']");
    public ILocator BotaoSalvarEndereco => _pagina.GetByRole(AriaRole.Button, new() { Name = "Salvar endereço" });

    public async Task PreencherEndereco(DadosDeEndereco dados)
    {
        await CampoCep.FillAsync(dados.CEP);
        await CampoEstado.FillAsync(dados.Estado);
        await CampoCidade.FillAsync(dados.Cidade);
        await CampoBairro.FillAsync(dados.Bairro);
        await CampoRua.FillAsync(dados.Rua);
        await CampoNumero.FillAsync(dados.Numero);
        if (dados.Complemento is not null)
            await CampoComplemento.FillAsync(dados.Complemento);
    }
}
