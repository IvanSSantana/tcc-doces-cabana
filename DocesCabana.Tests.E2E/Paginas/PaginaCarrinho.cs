using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCarrinho
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-carrinho");

    public PaginaCarrinho(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Carrinho");

    public ILocator Itens => Container.Locator(".item-carrinho");
    public ILocator MensagemVazia => Container.Locator(".carrinho-vazio");
    public ILocator Subtotal => Container.Locator(".subtotal-carrinho");
    public ILocator TotalDeItens => Container.Locator(".total-itens-carrinho");
    public ILocator BotaoFinalizar => Container.Locator(".botao-finalizar-carrinho");
    public ILocator ResultadoCarrinho => _pagina.Locator("#itens-carrinho");

    public ILocator ItemPeloProduto(Guid produtoId) =>
        Container.Locator($".item-carrinho[data-produto-id='{produtoId}']");

    public async Task AumentarQuantidade(Guid produtoId)
    {
        await ItemPeloProduto(produtoId).Locator(".botao-quantidade-carrinho.mais").ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DiminuirQuantidade(Guid produtoId)
    {
        await ItemPeloProduto(produtoId).Locator(".botao-quantidade-carrinho.menos").ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task Remover(Guid produtoId)
    {
        await ItemPeloProduto(produtoId).Locator(".botao-remover-carrinho").ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // Semeia o carrinho batendo direto na ação já testada em unidade
    // (CarrinhoController.Acrescentar), sem passar pela UI do cartão/página
    // do produto — essa integração só existe a partir da Fase 8 desta
    // feature. Roda como fetch dentro da própria página, reaproveitando o
    // cookie de autenticação e o token anti-falsificação que já estão
    // presentes em qualquer página (o formulário de favorito, no _Layout,
    // carrega @Html.AntiForgeryToken() sempre) — é a mesma prova de que o
    // caminho real funciona, não um atalho por fora dele.
    public async Task<int> SemearItem(string urlBase, Guid produtoId, int quantidade = 1)
    {
        var status = await _pagina.EvaluateAsync<int>(
            @"async ({ urlBase, produtoId, quantidade }) => {
                const token = document.querySelector(""input[name='__RequestVerificationToken']"").value;
                const corpo = new URLSearchParams();
                corpo.set('produtoId', produtoId);
                corpo.set('quantidade', quantidade);
                corpo.set('__RequestVerificationToken', token);
                const resposta = await fetch(urlBase + '/Carrinho/Acrescentar', {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: corpo.toString(),
                });
                return resposta.status;
            }",
            new { urlBase, produtoId = produtoId.ToString(), quantidade });

        return status;
    }
}
