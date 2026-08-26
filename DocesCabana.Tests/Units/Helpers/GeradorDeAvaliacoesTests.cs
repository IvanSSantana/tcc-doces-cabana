using DocesCabana.Domain.Entities;
using DocesCabana.MVC.Helpers;

namespace DocesCabana.Tests.Units.Helpers;

// Cobre o gerador de avaliações do seed (spec 014, RF-12/RF-13/RF-14) — sem
// banco, para poder ser chamado duas vezes na mesma execução e comparado
// (CA-16).
public class GeradorDeAvaliacoesTests
{
    private static List<Produto> GerarProdutos(int quantidade)
    {
        var subcategoriaId = Guid.NewGuid();
        return Enumerable.Range(1, quantidade)
            .Select(i => new Produto(subcategoriaId, $"Produto {i}", 10m, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m))
            .ToList();
    }

    private static List<Guid> GerarUsuarios(int quantidade) =>
        Enumerable.Range(1, quantidade).Select(_ => Guid.NewGuid()).ToList();

    [Fact]
    public void Dado_CemProdutos_Quando_Gerar_Entao_AMaiorParteDeveReceberAvaliacao()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        var produtosAvaliados = avaliacoes.Select(a => a.ProdutoId).Distinct().Count();

        // "Maior parte" (RF-12): bem mais da metade, sem fixar percentual
        // exato — o requisito não pede precisão, só que não seja um punhado.
        Assert.True(produtosAvaliados >= 60, $"Só {produtosAvaliados} de 100 produtos receberam avaliação.");
    }

    [Fact]
    public void Dado_CemProdutos_Quando_Gerar_Entao_ParteDeveFicarSemAvaliacaoNenhuma()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        var idsAvaliados = avaliacoes.Select(a => a.ProdutoId).ToHashSet();
        var produtosSemAvaliacao = produtos.Count(p => !idsAvaliados.Contains(p.ProdutoId));

        Assert.True(produtosSemAvaliacao > 0, "Nenhum produto ficou sem avaliação — RF-13 exige que parte fique.");
        Assert.True(produtosSemAvaliacao < produtos.Count, "Todos os produtos ficaram sem avaliação.");
    }

    [Fact]
    public void Dado_UmProdutoAvaliado_Quando_ContarAsAvaliacoes_Entao_DeveEstarEntreUmAQuatro()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        var porProduto = avaliacoes.GroupBy(a => a.ProdutoId).Select(g => g.Count());
        Assert.All(porProduto, quantidade => Assert.InRange(quantidade, 1, 4));
    }

    [Fact]
    public void Dado_AMesmaSemente_Quando_GerarDuasVezes_Entao_DeveProduzirOMesmoResultado()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var primeira = DbInitializer.GerarAvaliacoesMock(produtos, usuarios, semente: 42);
        var segunda = DbInitializer.GerarAvaliacoesMock(produtos, usuarios, semente: 42);

        var resumoPrimeira = primeira.Select(a => (a.ProdutoId, a.UsuarioId, a.Nota)).OrderBy(x => x).ToList();
        var resumoSegunda = segunda.Select(a => (a.ProdutoId, a.UsuarioId, a.Nota)).OrderBy(x => x).ToList();

        Assert.Equal(resumoPrimeira, resumoSegunda);
    }

    [Fact]
    public void Dado_SementesDiferentes_Quando_Gerar_Entao_PodeProduzirResultadosDiferentes()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var primeira = DbInitializer.GerarAvaliacoesMock(produtos, usuarios, semente: 1);
        var segunda = DbInitializer.GerarAvaliacoesMock(produtos, usuarios, semente: 2);

        var resumoPrimeira = primeira.Select(a => (a.ProdutoId, a.UsuarioId, a.Nota)).OrderBy(x => x).ToList();
        var resumoSegunda = segunda.Select(a => (a.ProdutoId, a.UsuarioId, a.Nota)).OrderBy(x => x).ToList();

        Assert.NotEqual(resumoPrimeira, resumoSegunda);
    }

    [Fact]
    public void Dado_QualquerProduto_Quando_Gerar_Entao_NenhumaPessoaAvaliaOMesmoProdutoDuasVezes()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        var grupos = avaliacoes.GroupBy(a => (a.ProdutoId, a.UsuarioId));
        Assert.All(grupos, g => Assert.Single(g));
    }

    [Fact]
    public void Dado_Avaliacoes_Quando_Gerar_Entao_NotasDevemFicarEnviesadasParaCima()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        var media = avaliacoes.Average(a => a.Nota);
        Assert.True(media >= 3.5, $"Média das notas geradas foi {media}, esperado enviesada para cima (>= 3.5).");
    }

    [Fact]
    public void Dado_Avaliacoes_Quando_Gerar_Entao_ParteDeveFicarSemComentario()
    {
        var produtos = GerarProdutos(100);
        var usuarios = GerarUsuarios(8);

        var avaliacoes = DbInitializer.GerarAvaliacoesMock(produtos, usuarios);

        Assert.Contains(avaliacoes, a => a.Comentario is null);
        Assert.Contains(avaliacoes, a => a.Comentario is not null);
    }
}
