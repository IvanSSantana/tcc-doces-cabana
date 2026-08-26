using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

// T038 (spec 022): confere na base semeada que os pedidos existem, têm
// itens, e que as quantidades vendidas diferem o bastante entre produtos
// para uma ordenação por venda ser visível — sem subir Identity inteiro
// (SemearPedidosDeExemplo não depende dele, só de DbContext).
public class DbInitializerPedidosIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_UsuariosEProdutosSemeados_Quando_SemearPedidosDeExemplo_Entao_DeveGravarPedidosComItensEPagamento()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtos = new List<Produto>();
        for (var i = 0; i < 25; i++)
            produtos.Add(await SemearProduto(subcategoriaId, $"Produto {i}", 10m + i));

        var usuarioIds = new List<Guid>();
        for (var i = 0; i < 8; i++)
            usuarioIds.Add(await SemearUsuario($"Cliente {i}", CpfValido(i)));

        await DbInitializer.SemearPedidosDeExemplo(Contexto, usuarioIds, produtos);

        var pedidos = await Contexto.Pedidos.Include(p => p.Itens).ToListAsync();
        Assert.NotEmpty(pedidos);
        Assert.All(pedidos, p => Assert.NotEmpty(p.Itens));

        var pagamentos = await Contexto.Pagamentos.ToListAsync();
        Assert.Equal(pedidos.Count, pagamentos.Count);

        // RN-05/CA-22: existe ao menos um pedido cancelado, para a
        // ordenação por venda ter o que excluir.
        Assert.Contains(pedidos, p => p.Status == PedidoStatus.Cancelado);

        // RF-24/CA-20: quantidades vendidas visivelmente diferentes entre
        // produtos, contando só pedido não cancelado (mesma regra que
        // ProdutoRepository.AplicarOrdenacao vai usar).
        var vendasPorProduto = pedidos
            .Where(p => p.Status != PedidoStatus.Cancelado)
            .SelectMany(p => p.Itens)
            .GroupBy(i => i.ProdutoId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));

        Assert.True(vendasPorProduto.Count >= 3, "Esperava vendas distribuídas em pelo menos três produtos.");
        Assert.True(vendasPorProduto.Values.Max() > vendasPorProduto.Values.Min(),
            "As quantidades vendidas deveriam diferir o bastante para uma ordenação por venda ser visível.");
    }

    private async Task<Produto> SemearProduto(Guid subcategoriaId, string nome, decimal preco)
    {
        var produto = new Produto(subcategoriaId, nome, preco, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }

    // CPFs válidos distintos — o índice único de Usuario.CPF não aceita
    // repetição (mesma restrição que SemearUsuario da base já respeita).
    private static string CpfValido(int indice) =>
        new[]
        {
            "52998224725", "11144477735", "39053344705", "45678912364",
            "01234567890", "12345678909", "98765432100", "11223344517",
        }[indice % 8];
}
