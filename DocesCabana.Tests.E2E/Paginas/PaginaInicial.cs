using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaInicial
{
    private readonly IPage _pagina;

    public PaginaInicial(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync(urlBase);

    public ILocator TituloDaVitrine => _pagina.Locator(".pagina-inicial-vitrine").Locator("xpath=preceding-sibling::h2[1]");
    public ILocator CardsDaVitrine => _pagina.Locator(".pagina-inicial-vitrine .card-produto");
    public ILocator BotaoFavoritar(int indice = 0) => CardsDaVitrine.Nth(indice).Locator(".botao-favorito-card");
    public ILocator PontosVisiveis => _pagina.Locator(".pagina-inicial-vitrine .ponto-indicador:visible");

    public ILocator FaixaDeConteudo => _pagina.Locator(".cabecalho-inferior section");
    public ILocator ItemDeCategoria => _pagina.Locator(".item-categoria-nav").First;
    public ILocator LinkDaCategoria => ItemDeCategoria.Locator(".link-nav");
    public ILocator PainelDoMenu => ItemDeCategoria.Locator(".submenu-categoria");
    public ILocator CartaoDoMenu => PainelDoMenu.Locator("ul");

    public async Task AbrirMenuDaCategoria() => await LinkDaCategoria.HoverAsync();

    public async Task<string?> CorDeFundoDoLink() =>
        await LinkDaCategoria.EvaluateAsync<string>("el => getComputedStyle(el.closest('.item-categoria-nav')).backgroundColor");

    public async Task<string?> CorDeFundoDoPainel() =>
        await PainelDoMenu.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

    public ILocator BarraDePesquisa => _pagina.Locator(".barra-pesquisa input[name='termo']");
    public ILocator BotaoPesquisar => _pagina.Locator(".botao-pesquisar");

    public async Task Buscar(string termo)
    {
        await BarraDePesquisa.FillAsync(termo);
        await BotaoPesquisar.ClickAsync();
    }
}
