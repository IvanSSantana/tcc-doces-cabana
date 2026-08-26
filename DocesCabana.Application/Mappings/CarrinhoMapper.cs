using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class CarrinhoMapper
{
    // Carrinho persistido: cada ItemCarrinho já carrega o Produto (Include
    // do repositório).
    public static CarrinhoDTO ToDTO(IEnumerable<ItemCarrinho> itens, CotacaoDeFreteDTO? cotacao = null) =>
        Montar(itens.Select(i => (i.Produto!, i.Quantidade)), cotacao);

    // Carrinho avulso (sessão): os pares vêm de fora, já resolvidos contra
    // o repositório de produto por quem chamou. cotacao tem padrão null —
    // os chamadores da 017 seguem compilando sem alteração (spec 021, plano
    // §6).
    public static CarrinhoDTO Montar(IEnumerable<(Produto Produto, short Quantidade)> pares, CotacaoDeFreteDTO? cotacao = null)
    {
        var linhas = pares.Select(par => new LinhaDoCarrinhoDTO
        {
            ProdutoId = par.Produto.ProdutoId,
            Nome = par.Produto.Nome,
            ImagemUrl = par.Produto.ImagemUrl,
            PrecoUnitario = par.Produto.Preco,
            Quantidade = par.Quantidade,
            ValorDaLinha = par.Produto.Preco * par.Quantidade,
            MotivoIndisponibilidade = ResolverMotivo(par.Produto.Status)
        }).ToList();

        return new CarrinhoDTO
        {
            Linhas = linhas,
            // RF-08/RF-17: só as linhas disponíveis somam.
            Subtotal = linhas.Where(l => l.Disponivel).Sum(l => l.ValorDaLinha),
            // RF-14: soma de quantidade, inclusive indisponível — é o que
            // fisicamente está no carrinho.
            TotalDeItens = linhas.Sum(l => (int)l.Quantidade),
            Cotacao = cotacao
        };
    }

    private static MotivoIndisponibilidade ResolverMotivo(ProdutoStatus status) =>
        status switch
        {
            ProdutoStatus.Inativo => MotivoIndisponibilidade.ForaDoCatalogo,
            ProdutoStatus.ForaDeEstoque => MotivoIndisponibilidade.ForaDeEstoque,
            _ => MotivoIndisponibilidade.Nenhum
        };
}
