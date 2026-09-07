using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCadastroProduto
{
    // Rótulo exato da opção no seletor de subcategoria, qualificado por
    // categoria desde a spec 012 (RF-28) — "Categoria › Subcategoria", não
    // só o nome da subcategoria. É a mesma subcategoria do produto curado
    // que o DbInitializer semeia ("Raspa Tacho").
    public const string SubcategoriaConhecida = "Doces › Raspa de Tachos";

    // PNG 1x1 mínimo, em memória (spec 027, T021) — sem arquivo no disco,
    // sem fixture para manter. Só precisa ser um PNG válido: o cadastro não
    // inspeciona bytes além do que a extensão e o Content-Type já verificam.
    private static readonly byte[] PngMinimo = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    private readonly IPage _pagina;
    private ILocator Formulario => _pagina.Locator("form.formulario-autenticacao");

    public PaginaCadastroProduto(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Admin/Produto/Cadastro");

    // Peso/Altura/Largura/Comprimento (spec 020, RF-02) são obrigatórios no
    // formulário desde então — parâmetros opcionais para não obrigar todo
    // teste já existente a conhecer o detalhe, com o mesmo valor-padrão dos
    // testes de unidade (ProdutoTests).
    //
    // imagemUrl saiu (spec 027, RF-01) — o campo de endereço não existe
    // mais. anexarImagem controla só se o arquivo é anexado: o teste de
    // CA-02 (sem imagem) precisa poder chamar Preencher sem anexar nada.
    public async Task Preencher(
        string nome, decimal preco, string subcategoria = SubcategoriaConhecida,
        decimal peso = 0.5m, decimal altura = 10m, decimal largura = 15m, decimal comprimento = 20m,
        bool anexarImagem = true)
    {
        var cultura = System.Globalization.CultureInfo.InvariantCulture;

        await Formulario.GetByLabel("Nome do Produto").FillAsync(nome);
        await Formulario.GetByLabel("Preço").FillAsync(preco.ToString("0.00", cultura));
        await Formulario.GetByLabel("Status").SelectOptionAsync(new SelectOptionValue { Label = "Ativo" });

        if (anexarImagem)
        {
            await Formulario.GetByLabel("Imagem").SetInputFilesAsync(new FilePayload
            {
                Name = "produto.png",
                MimeType = "image/png",
                Buffer = PngMinimo
            });
        }

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
    public ILocator ErroDeImagem => Formulario.Locator("#imagem + .mensagem-erro, #imagem ~ .mensagem-erro");
    public ILocator ErroGeral => Formulario.Locator(".resumo-erros .mensagem-erro");
}
