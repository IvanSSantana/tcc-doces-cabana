using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCadastroProduto
{
    // Nome da subcategoria semeada pelo DbInitializer desde a spec 003 —
    // "para que testes e E2E possam referenciar uma categoria/subcategoria
    // conhecida" (comentário original do próprio seed).
    public const string SubcategoriaConhecida = "Doces de Tacho";

    private readonly IPage _pagina;
    private ILocator Formulario => _pagina.Locator("form.formulario-autenticacao");

    public PaginaCadastroProduto(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Admin/Cadastro");

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
