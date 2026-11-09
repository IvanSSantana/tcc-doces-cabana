using DocesCabana.Tests.Integration.Infraestrutura;
using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class ModeloDeDadosIntegrationTests : InfraestruturaPostgresDescartavel
{
    [Fact]
    public async Task Dado_ProdutoComSubcategoriaInexistente_Quando_Salvar_Entao_DeveRecusar()
    {
        var produto = new Produto(Guid.NewGuid(), "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);

        Contexto.Produtos.Add(produto);

        await Assert.ThrowsAsync<DbUpdateException>(() => Contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Dado_UmFavoritoExistente_Quando_AdicionarOMesmoParNovamente_Entao_DeveRecusar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var usuarioId = await SemearUsuario();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);

        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId));
        await Contexto.SaveChangesAsync();

        Assert.Throws<InvalidOperationException>(() =>
            Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId)));
    }

    [Fact]
    public async Task Dado_UmEstoqueExistente_Quando_CriarSegundoEstoqueParaOMesmoProduto_Entao_DeveRecusar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);

        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        Contexto.Estoques.Add(new Estoque(produto.ProdutoId, 10));
        await Contexto.SaveChangesAsync();

        Assert.Throws<InvalidOperationException>(() =>
            Contexto.Estoques.Add(new Estoque(produto.ProdutoId, 5)));
    }
}
