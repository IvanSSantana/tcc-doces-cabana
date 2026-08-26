using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class AvaliacaoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_AvaliacoesComNotasDiferentes_Quando_BuscarPorProdutoOrdenandoPorMaiorNota_Entao_DeveTrazerDaMaiorParaAMenor()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        // Autores distintos: o índice único de Avaliacao(UsuarioId, ProdutoId)
        // (spec 014, RF-15) não permite a mesma pessoa avaliar o mesmo
        // produto mais de uma vez.
        var autorUmId = await SemearUsuario("Autor Um", "52998224725");
        var autorDoisId = await SemearUsuario("Autor Dois", "11144477735");
        var autorTresId = await SemearUsuario("Autor Três", "39053344705");

        await SemearAvaliacao(produto.ProdutoId, autorUmId, nota: 3);
        await SemearAvaliacao(produto.ProdutoId, autorDoisId, nota: 5);
        await SemearAvaliacao(produto.ProdutoId, autorTresId, nota: 1);

        var repositorio = new AvaliacaoRepository(Contexto);
        var avaliacoes = (await repositorio.BuscarPorProduto(produto.ProdutoId, OrdenacaoAvaliacao.MaiorNota, 10)).ToList();

        Assert.Equal([5, 3, 1], avaliacoes.Select(a => (int)a.Nota));
    }

    [Fact]
    public async Task Dado_AvaliacoesComDatasDiferentes_Quando_BuscarPorProdutoOrdenandoPorMaisRecentes_Entao_DeveTrazerAMaisNovaPrimeiro()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var autorAntigoId = await SemearUsuario("Autor Antigo", "52998224725");
        var autorRecenteId = await SemearUsuario("Autor Recente", "11144477735");

        var antiga = await SemearAvaliacao(produto.ProdutoId, autorAntigoId, nota: 4, comentario: "Antiga", dataCriacao: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var recente = await SemearAvaliacao(produto.ProdutoId, autorRecenteId, nota: 5, comentario: "Recente", dataCriacao: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var repositorio = new AvaliacaoRepository(Contexto);
        var avaliacoes = (await repositorio.BuscarPorProduto(produto.ProdutoId, OrdenacaoAvaliacao.MaisRecentes, 10)).ToList();

        Assert.Equal(recente.AvaliacaoId, avaliacoes[0].AvaliacaoId);
        Assert.Equal(antiga.AvaliacaoId, avaliacoes[1].AvaliacaoId);
    }

    [Fact]
    public async Task Dado_AvaliacoesComVotosDiferentes_Quando_BuscarPorProdutoOrdenandoPorRelevantes_Entao_DeveTrazerAMaisVotadaPrimeiro()
    {
        // RN-05: Relevantes ordena pela mais útil primeiro.
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var autorPoucoVotadaId = await SemearUsuario("Autor Pouco Votada", "52998224725");
        var autorMuitoVotadaId = await SemearUsuario("Autor Muito Votada", "87654321937");
        var votanteUmId = await SemearUsuario("Votante Um", "11144477735");
        var votanteDoisId = await SemearUsuario("Votante Dois", "39053344705");

        var poucoVotada = await SemearAvaliacao(produto.ProdutoId, autorPoucoVotadaId, nota: 4, comentario: "Pouco votada");
        var muitoVotada = await SemearAvaliacao(produto.ProdutoId, autorMuitoVotadaId, nota: 3, comentario: "Muito votada");

        Contexto.VotosUteis.Add(new VotoUtil(muitoVotada.AvaliacaoId, votanteUmId));
        Contexto.VotosUteis.Add(new VotoUtil(muitoVotada.AvaliacaoId, votanteDoisId));
        Contexto.VotosUteis.Add(new VotoUtil(poucoVotada.AvaliacaoId, votanteUmId));
        await Contexto.SaveChangesAsync();

        var repositorio = new AvaliacaoRepository(Contexto);
        var avaliacoes = (await repositorio.BuscarPorProduto(produto.ProdutoId, OrdenacaoAvaliacao.Relevantes, 10)).ToList();

        Assert.Equal(muitoVotada.AvaliacaoId, avaliacoes[0].AvaliacaoId);
        Assert.Equal(poucoVotada.AvaliacaoId, avaliacoes[1].AvaliacaoId);
    }

    [Fact]
    public async Task Dado_AvaliacoesComNotasVariadas_Quando_ContarPorNota_Entao_DeveAgruparPorNota()
    {
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5.00m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        var autorUmId = await SemearUsuario("Autor Um", "52998224725");
        var autorDoisId = await SemearUsuario("Autor Dois", "11144477735");
        var autorTresId = await SemearUsuario("Autor Três", "39053344705");

        await SemearAvaliacao(produto.ProdutoId, autorUmId, nota: 5);
        await SemearAvaliacao(produto.ProdutoId, autorDoisId, nota: 5);
        await SemearAvaliacao(produto.ProdutoId, autorTresId, nota: 3);

        var repositorio = new AvaliacaoRepository(Contexto);
        var contagem = await repositorio.ContarPorNota(produto.ProdutoId);
        var total = await repositorio.ContarPorProduto(produto.ProdutoId);

        Assert.Equal(2, contagem[5]);
        Assert.Equal(1, contagem[3]);
        Assert.Equal(3, total);
    }
}
