using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class ModeloDeDadosIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_ProdutoComSubcategoriaInexistente_Quando_Salvar_Entao_DeveRecusar()
    {
        var produto = new Produto(Guid.NewGuid(), "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");

        Contexto.Produtos.Add(produto);

        await Assert.ThrowsAsync<DbUpdateException>(() => Contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Dado_UmFavoritoExistente_Quando_AdicionarOMesmoParNovamente_Entao_DeveRecusar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var usuarioId = await SemearUsuario();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");

        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId));
        await Contexto.SaveChangesAsync();

        // A chave primária de Favorito é o par (ProdutoId, UsuarioId) — um
        // segundo Add com o mesmo par tenta reinserir a mesma PK, que o
        // rastreador de mudanças já recusa antes de qualquer ida ao banco.
        Assert.Throws<InvalidOperationException>(() =>
            Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId)));
    }

    [Fact]
    public async Task Dado_UmEstoqueExistente_Quando_CriarSegundoEstoqueParaOMesmoProduto_Entao_DeveRecusar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");

        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        Contexto.Estoques.Add(new Estoque(produto.ProdutoId, 10));
        await Contexto.SaveChangesAsync();

        // A chave primária de Estoque é o próprio ProdutoId (1:1 por chave
        // compartilhada) — um segundo Add com o mesmo produto tenta reinserir
        // a mesma PK, que o rastreador de mudanças já recusa antes do banco.
        Assert.Throws<InvalidOperationException>(() =>
            Contexto.Estoques.Add(new Estoque(produto.ProdutoId, 5)));
    }

    [Fact]
    public async Task Dado_ProdutoComSubcategoria_Quando_ConsultarSemInclude_Entao_NavegacaoDeveVirNula()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        var encontrado = await Contexto.Produtos
            .AsNoTracking()
            .FirstAsync(p => p.ProdutoId == produto.ProdutoId);

        Assert.Null(encontrado.Subcategoria);
    }

    [Fact]
    public async Task Dado_ProdutoComSubcategoria_Quando_ConsultarComInclude_Entao_NavegacaoDeveVirPreenchida()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        var encontrado = await Contexto.Produtos
            .AsNoTracking()
            .Include(p => p.Subcategoria)
            .FirstAsync(p => p.ProdutoId == produto.ProdutoId);

        Assert.NotNull(encontrado.Subcategoria);
        Assert.Equal("Doces de Tacho", encontrado.Subcategoria!.Nome);
    }
}
