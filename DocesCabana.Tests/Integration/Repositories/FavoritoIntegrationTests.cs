using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

// Cobre a chave composta de Favorito(ProdutoId, UsuarioId), criada desde a
// migration da spec 003 — é ela quem garante RN-01 (par único) no banco,
// sem barreira de validação adicional (spec 015, plano §10).
public class FavoritoIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_UmParJaFavoritado_Quando_TentarFavoritarDeNovo_Entao_DeveSerRecusado()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId));
        await Contexto.SaveChangesAsync();

        // (ProdutoId, UsuarioId) é a própria chave primária de Favorito, não
        // um índice único à parte (como em Avaliacao) — o ChangeTracker já
        // recusa a segunda instância em memória, antes de chegar ao banco.
        Assert.Throws<InvalidOperationException>(() =>
            Contexto.Favoritos.Add(new Favorito(produto.ProdutoId, usuarioId)));
    }

    [Fact]
    public async Task Dado_FavoritosDeDuasPessoas_Quando_BuscarPorUsuario_Entao_NaoDeveTrazerODaOutra()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var usuarioUmId = await SemearUsuario("Cliente Um", "52998224725");
        var usuarioDoisId = await SemearUsuario("Cliente Dois", "11144477735");

        Contexto.Favoritos.Add(new Favorito(produtoUm.ProdutoId, usuarioUmId));
        Contexto.Favoritos.Add(new Favorito(produtoDois.ProdutoId, usuarioDoisId));
        await Contexto.SaveChangesAsync();

        var repositorio = new FavoritoRepository(Contexto);
        var favoritosDoUm = await repositorio.BuscarPorUsuario(usuarioUmId);

        Assert.Single(favoritosDoUm);
        Assert.Equal(produtoUm.ProdutoId, favoritosDoUm[0].ProdutoId);
    }

    [Fact]
    public async Task Dado_AMesmaPessoa_Quando_FavoritarProdutosDiferentes_Entao_DeveAceitarOsDois()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.Favoritos.Add(new Favorito(produtoUm.ProdutoId, usuarioId));
        Contexto.Favoritos.Add(new Favorito(produtoDois.ProdutoId, usuarioId));
        await Contexto.SaveChangesAsync();

        var repositorio = new FavoritoRepository(Contexto);
        var favoritos = await repositorio.BuscarPorUsuario(usuarioId);

        Assert.Equal(2, favoritos.Count);
    }

    [Fact]
    public async Task Dado_IdentificadoresMisturados_Quando_IdsPorUsuario_Entao_DeveDevolverSoOsFavoritados()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoFavoritado = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoNaoFavoritado = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoFavoritado, produtoNaoFavoritado);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.Favoritos.Add(new Favorito(produtoFavoritado.ProdutoId, usuarioId));
        await Contexto.SaveChangesAsync();

        var repositorio = new FavoritoRepository(Contexto);
        var ids = await repositorio.IdsPorUsuario(usuarioId, [produtoFavoritado.ProdutoId, produtoNaoFavoritado.ProdutoId]);

        Assert.Single(ids);
        Assert.Contains(produtoFavoritado.ProdutoId, ids);
    }
}
