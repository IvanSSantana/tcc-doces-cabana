using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class CarrinhoMapper
{
    public static CarrinhoDTO ToDTO(
        IEnumerable<ItemCarrinho> itens, CotacaoDeFreteDTO? cotacao = null, PromocoesVigentes? promocoes = null) =>
        Montar(itens.Select(i => (i.Produto!, i.Quantidade)), cotacao, promocoes);

    public static CarrinhoDTO Montar(
        IEnumerable<(Produto Produto, short Quantidade)> pares,
        CotacaoDeFreteDTO? cotacao = null,
        PromocoesVigentes? promocoes = null)
    {
        promocoes ??= PromocoesVigentes.Nenhuma;

        var linhas = pares.Select(par => MontarLinha(par.Produto, par.Quantidade, promocoes)).ToList();

        return new CarrinhoDTO
        {
            Linhas = linhas,
            Subtotal = linhas.Where(l => l.Disponivel).Sum(l => l.ValorDaLinha),
            TotalDeItens = linhas.Sum(l => (int)l.Quantidade),
            Cotacao = cotacao
        };
    }

    private static LinhaDoCarrinhoDTO MontarLinha(Produto produto, short quantidade, PromocoesVigentes promocoes)
    {
        var motivo = ResolverMotivo(produto.Status);

        var desconto = motivo == MotivoIndisponibilidade.Nenhum
            ? promocoes.MelhorPara(produto.ProdutoId, produto.SubcategoriaId, produto.Preco, quantidade)
            : null;

        return new LinhaDoCarrinhoDTO
        {
            ProdutoId = produto.ProdutoId,
            SubcategoriaId = produto.SubcategoriaId,
            Nome = produto.Nome,
            ImagemUrl = produto.ImagemUrl,
            PrecoUnitario = produto.Preco,
            Quantidade = quantidade,
            DescontoDaPromocao = desconto?.Valor ?? 0m,
            RotuloDaPromocao = desconto?.Rotulo,
            ValorDaLinha = produto.Preco * quantidade - (desconto?.Valor ?? 0m),
            MotivoIndisponibilidade = motivo,
            Peso = produto.Peso,
            Altura = produto.Altura,
            Largura = produto.Largura,
            Comprimento = produto.Comprimento
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
