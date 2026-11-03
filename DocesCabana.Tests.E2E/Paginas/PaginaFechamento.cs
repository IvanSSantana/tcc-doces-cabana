using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaFechamento
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator("#itens-carrinho");

    public PaginaFechamento(IPage pagina) => _pagina = pagina;

    public ILocator PassoAtivo => Container.Locator(".passo-fechamento--ativo a");
    public ILocator LinkEntrar => Container.Locator(".passo-conta-fechamento").GetByRole(AriaRole.Link, new() { Name = "Entrar" });
    public ILocator LinkCriarConta => Container.Locator(".passo-conta-fechamento").GetByRole(AriaRole.Link, new() { Name = "Criar conta" });

    public ILocator ListaDeEnderecos => Container.Locator(".endereco-fechamento");
    public ILocator EnderecoSelecionado => Container.Locator(".endereco-fechamento--selecionado a");
    public ILocator DetalhesNovoEndereco => Container.Locator(".novo-endereco-fechamento");
    public ILocator ListaDeOpcoesDeEntrega => Container.Locator(".lista-opcoes-frete-fechamento .opcao-frete");
    public ILocator OpcaoDeEntregaSelecionada => Container.Locator(".opcao-frete--selecionada a");
    public ILocator LinkContinuarParaPagamento => Container.GetByRole(AriaRole.Link, new() { Name = "Continuar para pagamento" });

    public ILocator FormularioPagamento => Container.Locator(".formulario-pagamento-fechamento");
    public ILocator BotaoConfirmarPedido => FormularioPagamento.GetByRole(AriaRole.Button, new() { Name = "Confirmar pedido" });

    public async Task CadastrarEnderecoAqui(string cep, string rua, string numero)
    {
        var formulario = DetalhesNovoEndereco.Locator("form.formulario-endereco");

        if (!await formulario.IsVisibleAsync())
            await DetalhesNovoEndereco.Locator("summary").ClickAsync();

        await formulario.Locator("[data-campo-cep]").FillAsync(cep);
        await formulario.GetByLabel("Estado").FillAsync("SP");
        await formulario.GetByLabel("Cidade").FillAsync("Cidade Teste");
        await formulario.GetByLabel("Bairro").FillAsync("Bairro Teste");
        await formulario.GetByLabel("Rua").FillAsync(rua);
        await formulario.GetByLabel("Número").FillAsync(numero);
        await formulario.GetByRole(AriaRole.Button, new() { Name = "Salvar endereço" }).ClickAsync();
    }
}
