using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCadastroProduto
{
    // Rótulo exato da opção no seletor de subcategoria, qualificado por
    // categoria desde a spec 012 (RF-28) — "Categoria › Subcategoria", não
    // só o nome da subcategoria. É a mesma subcategoria do produto curado
    // que o DbInitializer semeia ("Raspa Tacho").
    public const string SubcategoriaConhecida = "Doces › Raspa de Tachos";

    private readonly IPage _pagina;
    private ILocator Formulario => _pagina.Locator("form.formulario-autenticacao");

    public PaginaCadastroProduto(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Admin/Produto/Cadastro");

    public async Task Preencher(string nome, decimal preco, string imagemUrl, string subcategoria = SubcategoriaConhecida)
    {
        await Formulario.GetByLabel("Nome do Produto").FillAsync(nome);
        await Formulario.GetByLabel("Preço").FillAsync(preco.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
        await Formulario.GetByLabel("Status").SelectOptionAsync(new SelectOptionValue { Label = "Ativo" });
        await Formulario.GetByLabel("Imagem (URL)").FillAsync(imagemUrl);
        await Formulario.GetByLabel("Subcategoria").SelectOptionAsync(new SelectOptionValue { Label = subcategoria });
    }

    public async Task Enviar() =>
        await Formulario.GetByRole(AriaRole.Button, new() { Name = "Cadastrar Produto" }).ClickAsync();

    public ILocator MensagemDeConfirmacao => Formulario.Locator(".resumo-sucesso .mensagem-sucesso");
    public ILocator ErroDePreco => Formulario.Locator("span[data-valmsg-for='Preco']");
}
