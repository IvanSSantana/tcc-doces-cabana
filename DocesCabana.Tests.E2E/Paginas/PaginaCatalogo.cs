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
    public ILocator MensagemVazia => Container.Locator(".catalogo-vazio");
    public ILocator Paginacao => Container.Locator(".paginacao-catalogo");
    public ILocator LinkPaginaAtual => Container.Locator(".link-paginacao--atual");

    // CheckAsync() só espera a própria marcação — o reenvio do formulário
    // (onchange="this.form.submit()") é efeito colateral de script, e sem
    // esperar a navegação explicitamente a leitura seguinte da grade corre
    // risco de pegar a página antiga a meio caminho da troca.
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

    public async Task IrParaPagina(int pagina) =>
        await Container.GetByRole(AriaRole.Link, new() { Name = pagina.ToString(), Exact = true }).ClickAsync();
}
