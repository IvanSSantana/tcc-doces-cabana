using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

// Cobre a chave composta de ItemCarrinho(UsuarioId, ProdutoId), mesmo padrão
// de Favorito (spec 015) — é ela quem garante RN-01 (um produto por linha)
// no banco, sem barreira de validação adicional.
public class CarrinhoIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_UmParJaNoCarrinho_Quando_TentarAdicionarDeNovo_Entao_DeveSerRecusado()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produto.ProdutoId, 2));
        await Contexto.SaveChangesAsync();

        // (UsuarioId, ProdutoId) é a própria chave primária de ItemCarrinho,
        // não um índice único à parte — o ChangeTracker já recusa a segunda
        // instância em memória, antes de chegar ao banco.
        Assert.Throws<InvalidOperationException>(() =>
            Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produto.ProdutoId, 1)));
    }

    [Fact]
    public async Task Dado_CarrinhosDeDuasPessoas_Quando_BuscarPorUsuario_Entao_NaoDeveTrazerODaOutra()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var usuarioUmId = await SemearUsuario("Cliente Um", "52998224725");
        var usuarioDoisId = await SemearUsuario("Cliente Dois", "11144477735");

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioUmId, produtoUm.ProdutoId, 1));
        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioDoisId, produtoDois.ProdutoId, 1));
        await Contexto.SaveChangesAsync();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var carrinhoDoUm = await repositorio.BuscarPorUsuario(usuarioUmId);

        Assert.Single(carrinhoDoUm);
        Assert.Equal(produtoUm.ProdutoId, carrinhoDoUm[0].ProdutoId);
    }

    [Fact]
    public async Task Dado_BuscarPorUsuario_Quando_Consultar_Entao_DeveTrazerOProdutoIncluido()
    {
        // O carrinho precisa do produto inteiro (nome, imagem, preço,
        // status) para montar a tela — sem Include, cada linha viria com
        // Produto nulo.
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produto.ProdutoId, 1));
        await Contexto.SaveChangesAsync();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var carrinho = await repositorio.BuscarPorUsuario(usuarioId);

        Assert.NotNull(carrinho[0].Produto);
        Assert.Equal("Brigadeiro", carrinho[0].Produto!.Nome);
    }

    [Fact]
    public async Task Dado_AMesmaPessoa_Quando_AdicionarProdutosDiferentes_Entao_DeveAceitarOsDois()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produtoUm.ProdutoId, 1));
        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produtoDois.ProdutoId, 1));
        await Contexto.SaveChangesAsync();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var carrinho = await repositorio.BuscarPorUsuario(usuarioId);

        Assert.Equal(2, carrinho.Count);
    }

    [Fact]
    public async Task Dado_Buscar_Quando_ParExiste_Entao_DeveEncontrar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produto.ProdutoId, 4));
        await Contexto.SaveChangesAsync();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var item = await repositorio.Buscar(usuarioId, produto.ProdutoId);

        Assert.NotNull(item);
        Assert.Equal(4, item!.Quantidade);
    }

    [Fact]
    public async Task Dado_Buscar_Quando_ParNaoExiste_Entao_DeveDevolverNulo()
    {
        var usuarioId = await SemearUsuario();
        var repositorio = new ItemCarrinhoRepository(Contexto);

        var item = await repositorio.Buscar(usuarioId, Guid.NewGuid());

        Assert.Null(item);
    }

    [Fact]
    public async Task Dado_DoisItensComQuantidadesDiferentes_Quando_ContarItens_Entao_DeveSomarAsQuantidadesNaoAsLinhas()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produtoUm.ProdutoId, 3));
        Contexto.ItensCarrinho.Add(new ItemCarrinho(usuarioId, produtoDois.ProdutoId, 2));
        await Contexto.SaveChangesAsync();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var total = await repositorio.ContarItens(usuarioId);

        Assert.Equal(5, total);
    }

    [Fact]
    public async Task Dado_ItemAdicionadoPeloRepositorio_Quando_Remover_Entao_DeveSairDoBanco()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var usuarioId = await SemearUsuario();

        var repositorio = new ItemCarrinhoRepository(Contexto);
        var item = new ItemCarrinho(usuarioId, produto.ProdutoId, 1);
        await repositorio.Adicionar(item);
        await Contexto.SaveChangesAsync();

        repositorio.Remover(item);
        await Contexto.SaveChangesAsync();

        var carrinho = await repositorio.BuscarPorUsuario(usuarioId);
        Assert.Empty(carrinho);
    }
}
