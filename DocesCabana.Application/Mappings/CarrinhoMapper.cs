using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class CarrinhoMapper
{
    public static CarrinhoDTO ToDTO(IEnumerable<ItemCarrinho> itens, CotacaoDeFreteDTO? cotacao = null) =>
        Montar(itens.Select(i => (i.Produto!, i.Quantidade)), cotacao);

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
            MotivoIndisponibilidade = ResolverMotivo(par.Produto.Status),
            Peso = par.Produto.Peso,
            Altura = par.Produto.Altura,
            Largura = par.Produto.Largura,
            Comprimento = par.Produto.Comprimento
        }).ToList();

        return new CarrinhoDTO
        {
            Linhas = linhas,
            Subtotal = linhas.Where(l => l.Disponivel).Sum(l => l.ValorDaLinha),
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
