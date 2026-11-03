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
    public ILocator ResultadoCatalogo => Container.Locator("#resultado-catalogo");
    public ILocator SeletorDeOrdenacaoNoResultado => ResultadoCatalogo.Locator("#select-ordenacao");
    public ILocator LinkDeCategoria(string nome) => Container.Locator(".link-categoria-catalogo", new() { HasText = nome });

    public ILocator BarraDePesquisa => _pagina.Locator(".barra-pesquisa input[name='termo']");
    public ILocator EtiquetaDeBusca => ResultadoCatalogo.Locator(".etiqueta-busca-catalogo");
    public ILocator BotaoRemoverBusca => ResultadoCatalogo.Locator(".botao-remover-busca");
    public ILocator CaixaDeSubcategoriaPeloNome(string nome) =>
        Container.Locator(".opcao-filtro-catalogo", new() { HasText = nome }).Locator("input");

    public async Task MarcarSubcategoriaPeloNome(string nome)
    {
        await Container.Locator(".opcao-filtro-catalogo", new() { HasText = nome }).Locator("input").CheckAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task GarantirSubcategoriaVisivel(string nome)
    {
        if (!await CaixaDeSubcategoriaPeloNome(nome).IsVisibleAsync())
            await VerTodas.ClickAsync();
    }

    public async Task MarcarSemAcucar()
    {
        await CaixaSemAcucar.CheckAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task IrParaPagina(int pagina)
    {
        await Container.GetByRole(AriaRole.Link, new() { Name = pagina.ToString(), Exact = true }).ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
