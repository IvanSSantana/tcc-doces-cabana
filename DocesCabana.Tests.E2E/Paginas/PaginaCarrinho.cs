using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCarrinho
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-carrinho");

    public PaginaCarrinho(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Carrinho");

    public ILocator MensagemVazia => Container.Locator(".carrinho-vazio");
    public ILocator BotaoFinalizar => Container.Locator(".botao-finalizar-carrinho");

    public ILocator CampoCep => Container.Locator("#cep-carrinho");

    public ILocator ItemPeloProduto(Guid produtoId) =>
        Container.Locator($".item-carrinho[data-produto-id='{produtoId}']");

    public async Task AumentarQuantidade(Guid produtoId)
    {
        await ItemPeloProduto(produtoId).Locator(".botao-quantidade-carrinho.mais").ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task Remover(Guid produtoId)
    {
        await ItemPeloProduto(produtoId).Locator(".botao-remover-carrinho").ClickAsync();
        await _pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

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
