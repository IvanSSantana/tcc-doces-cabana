using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCatalogo
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-catalogo");

    public PaginaCatalogo(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase, string? apelido = null) =>
        await _pagina.GotoAsync(apelido is null ? $"{urlBase}/Catalogo" : $"{urlBase}/Catalogo/{apelido}");

    public ILocator Trilha => Container.Locator(".trilha-catalogo");
    public ILocator Categorias => Container.Locator(".link-categoria-catalogo");
    public ILocator CategoriaAtiva => Container.Locator(".link-categoria-catalogo--ativa");
    // Filho direto do fieldset: exclui as que estão dentro do <details>
    // "Ver todas", que existem no DOM mesmo fechado (RF-10).
    public ILocator CaixasDeSubcategoria => Container.Locator(".filtro-subcategorias > .opcao-filtro-catalogo input");
    public ILocator VerTodas => Container.Locator(".ver-todas-subcategorias summary");
    public ILocator CaixaSemAcucar => Container.Locator(".filtro-sem-acucar input");
    public ILocator SeletorDeOrdenacao => Container.Locator("#select-ordenacao");
    public ILocator Cards => Container.Locator(".card-produto");
    public ILocator Grade => Container.Locator(".grade-produtos");
    public ILocator MensagemVazia => Container.Locator(".catalogo-vazio");
    public ILocator Paginacao => Container.Locator(".paginacao-catalogo");
    public ILocator LinkPaginaAtual => Container.Locator(".link-paginacao--atual");
    public ILocator Contagem => Container.Locator(".contagem-catalogo");
    // Alvo do foco depois da troca parcial (spec 014, RF-18) e ancestral que
    // a atualização sem recarga substitui (RF-01).
    public ILocator ResultadoCatalogo => Container.Locator("#resultado-catalogo");
    public ILocator SeletorDeOrdenacaoNoResultado => ResultadoCatalogo.Locator("#select-ordenacao");
    public ILocator LinkDeCategoria(string nome) => Container.Locator(".link-categoria-catalogo", new() { HasText = nome });
    // A caixa em si não tem texto (é um <input>) — para achar por nome é
    // preciso passar pelo rótulo que a envolve, mesmo caminho que
    // MarcarSubcategoriaPeloNome já usa.
    public ILocator CaixaDeSubcategoriaPeloNome(string nome) =>
        Container.Locator(".opcao-filtro-catalogo", new() { HasText = nome }).Locator("input");

    // CheckAsync() só espera a própria marcação — o reenvio do formulário
    // (onchange="this.form.requestSubmit()", spec 014) é efeito colateral de
    // script, e sem esperar a rede explicitamente a leitura seguinte da
    // grade corre risco de pegar o resultado antigo a meio caminho da troca.
    public async Task MarcarSubcategoriaPeloNome(string nome)
    {
        await Container.Locator(".opcao-filtro-catalogo", new() { HasText = nome }).Locator("input").CheckAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task MarcarSemAcucar()
    {
        await CaixaSemAcucar.CheckAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // A troca de página passou a ser parcial (spec 014, RF-01): não há mais
    // navegação para o Playwright esperar sozinho, só a rede.
    public async Task IrParaPagina(int pagina)
    {
        await Container.GetByRole(AriaRole.Link, new() { Name = pagina.ToString(), Exact = true }).ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
