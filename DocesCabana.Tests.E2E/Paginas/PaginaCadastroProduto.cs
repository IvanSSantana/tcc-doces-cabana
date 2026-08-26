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

    // Peso/Altura/Largura/Comprimento (spec 020, RF-02) são obrigatórios no
    // formulário desde então — parâmetros opcionais para não obrigar todo
    // teste já existente a conhecer o detalhe, com o mesmo valor-padrão dos
    // testes de unidade (ProdutoTests).
    public async Task Preencher(
        string nome, decimal preco, string imagemUrl, string subcategoria = SubcategoriaConhecida,
        decimal peso = 0.5m, decimal altura = 10m, decimal largura = 15m, decimal comprimento = 20m)
    {
        var cultura = System.Globalization.CultureInfo.InvariantCulture;

        await Formulario.GetByLabel("Nome do Produto").FillAsync(nome);
        await Formulario.GetByLabel("Preço").FillAsync(preco.ToString("0.00", cultura));
        await Formulario.GetByLabel("Status").SelectOptionAsync(new SelectOptionValue { Label = "Ativo" });
        await Formulario.GetByLabel("Imagem (URL)").FillAsync(imagemUrl);
        await Formulario.GetByLabel("Subcategoria").SelectOptionAsync(new SelectOptionValue { Label = subcategoria });
        await Formulario.GetByLabel("Peso (kg)").FillAsync(peso.ToString("0.000", cultura));
        await Formulario.GetByLabel("Altura (cm)").FillAsync(altura.ToString("0.0", cultura));
        await Formulario.GetByLabel("Largura (cm)").FillAsync(largura.ToString("0.0", cultura));
        await Formulario.GetByLabel("Comprimento (cm)").FillAsync(comprimento.ToString("0.0", cultura));
    }

    public async Task Enviar() =>
        await Formulario.GetByRole(AriaRole.Button, new() { Name = "Cadastrar Produto" }).ClickAsync();

    public ILocator MensagemDeConfirmacao => Formulario.Locator(".resumo-sucesso .mensagem-sucesso");
    public ILocator ErroDePreco => Formulario.Locator("span[data-valmsg-for='Preco']");
}
