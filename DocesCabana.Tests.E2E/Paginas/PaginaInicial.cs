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
    // :visible é seletor próprio do Playwright, não CSS padrão — necessário
    // porque o carrossel esconde os pontos além do índice máximo com
    // `display: none` via script, sem remover do DOM (RF-08, spec 013).
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

    // Busca por nome de produto (spec 016) — presente no cabeçalho de
    // qualquer página, não só da home.
    public ILocator BarraDePesquisa => _pagina.Locator(".barra-pesquisa input[name='termo']");
    public ILocator BotaoPesquisar => _pagina.Locator(".botao-pesquisar");

    public async Task Buscar(string termo)
    {
        await BarraDePesquisa.FillAsync(termo);
        await BotaoPesquisar.ClickAsync();
    }
}
