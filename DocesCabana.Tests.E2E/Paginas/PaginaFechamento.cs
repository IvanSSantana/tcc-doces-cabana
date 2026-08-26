using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

/// <summary>
/// Os passos do fechamento (spec 022) — todos vivem dentro de
/// <c>#itens-carrinho</c>, a mesma tela do carrinho (<see cref="PaginaCarrinho"/>).
/// </summary>
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

        // <details> já vem aberto quando não há nenhum endereço (server-
        // rendered) — clicar no <summary> de qualquer jeito arriscaria
        // fechar em vez de abrir.
        if (!await formulario.IsVisibleAsync())
            await DetalhesNovoEndereco.Locator("summary").ClickAsync();

        await formulario.Locator("[data-campo-cep]").FillAsync(cep);
        // Sem script no navegador de teste que roda sem CEP-lookup ligado
        // (a máscara/preenchimento automático é conveniência visual, spec
        // 018) — preenche os campos obrigatórios manualmente.
        await formulario.GetByLabel("Estado").FillAsync("SP");
        await formulario.GetByLabel("Cidade").FillAsync("Cidade Teste");
        await formulario.GetByLabel("Bairro").FillAsync("Bairro Teste");
        await formulario.GetByLabel("Rua").FillAsync(rua);
        await formulario.GetByLabel("Número").FillAsync(numero);
        await formulario.GetByRole(AriaRole.Button, new() { Name = "Salvar endereço" }).ClickAsync();
    }
}
