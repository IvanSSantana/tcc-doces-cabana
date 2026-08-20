using DocesCabana.Application.DTOs;
using DocesCabana.MVC.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace DocesCabana.Tests.Units.ViewComponents;

public class VitrineProdutosTests
{
    private static List<ProdutoDTO> GerarProdutos(int quantidade) =>
        Enumerable.Range(1, quantidade)
            .Select(i => new ProdutoDTO { ProdutoId = Guid.NewGuid(), Nome = $"Produto {i}" })
            .ToList();

    private static List<ProdutoDTO> Invocar(IEnumerable<ProdutoDTO> produtos, int? limite = null)
    {
        var componente = new VitrineProdutosViewComponent();
        var resultado = limite.HasValue
            ? componente.Invoke(produtos, limite.Value)
            : componente.Invoke(produtos);

        var viewResult = Assert.IsAssignableFrom<ViewViewComponentResult>(resultado);
        return Assert.IsAssignableFrom<List<ProdutoDTO>>(viewResult.ViewData!.Model);
    }

    [Fact]
    public void Dado_NoventaENoveProdutos_Quando_InvocarComOLimitePadrao_Entao_DeveDevolverNoMaximoOito()
    {
        var produtos = GerarProdutos(99);

        var resultado = Invocar(produtos);

        Assert.Equal(8, resultado.Count);
    }

    [Fact]
    public void Dado_MenosProdutosQueOLimite_Quando_Invocar_Entao_DeveDevolverTodos()
    {
        var produtos = GerarProdutos(3);

        var resultado = Invocar(produtos);

        Assert.Equal(3, resultado.Count);
    }

    [Fact]
    public void Dado_UmLimiteExplicito_Quando_Invocar_Entao_DeveRespeitarOLimiteRecebido()
    {
        var produtos = GerarProdutos(20);

        var resultado = Invocar(produtos, limite: 5);

        Assert.Equal(5, resultado.Count);
    }
}
