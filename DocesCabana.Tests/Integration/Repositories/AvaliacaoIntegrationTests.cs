using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

// Cobre o índice único de Avaliacao(UsuarioId, ProdutoId) — a barreira que a
// spec 014 (RF-15/RN-01) usa em vez de validação de formulário, porque não
// existe entrada de usuário para validar enquanto a tela de escrever
// avaliação não existir (plano 014 §10).
public class AvaliacaoIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_PessoaQueJaAvaliouUmProduto_Quando_RegistrarSegundaAvaliacaoDoMesmoProduto_Entao_DeveSerRecusada()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var autorId = await SemearUsuario();

        await SemearAvaliacao(produto.ProdutoId, autorId, nota: 5);

        var segunda = new Avaliacao(autorId, produto.ProdutoId, 2, "Mudei de ideia.");
        Contexto.Avaliacoes.Add(segunda);

        await Assert.ThrowsAsync<DbUpdateException>(() => Contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Dado_PessoasDiferentes_Quando_AvaliaremOMesmoProduto_Entao_DeveAceitarAsDuas()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var autorUmId = await SemearUsuario("Cliente Um", "52998224725");
        var autorDoisId = await SemearUsuario("Cliente Dois", "11144477735");

        await SemearAvaliacao(produto.ProdutoId, autorUmId, nota: 5);
        await SemearAvaliacao(produto.ProdutoId, autorDoisId, nota: 3);

        var total = await Contexto.Avaliacoes.CountAsync(a => a.ProdutoId == produto.ProdutoId);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task Dado_AMesmaPessoa_Quando_AvaliarProdutosDiferentes_Entao_DeveAceitarAsDuas()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produtoUm = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg");
        var produtoDois = new Produto(subcategoriaId, "Beijinho", 5.00m, "https://imagem.com/beijinho.jpg");
        Contexto.Produtos.AddRange(produtoUm, produtoDois);
        await Contexto.SaveChangesAsync();
        var autorId = await SemearUsuario();

        await SemearAvaliacao(produtoUm.ProdutoId, autorId, nota: 5);
        await SemearAvaliacao(produtoDois.ProdutoId, autorId, nota: 4);

        var total = await Contexto.Avaliacoes.CountAsync(a => a.UsuarioId == autorId);
        Assert.Equal(2, total);
    }
}
