using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.Tests.Integration.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class BannerRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
{
    private static readonly DateOnly Hoje = new(2026, 12, 10);
    private const string Imagem = "https://exemplo.com/banner.png";

    private BannerRepository Repositorio => new(Contexto);

    private async Task Salvar(params Banner[] banners)
    {
        foreach (var banner in banners)
            await Repositorio.Adicionar(banner);

        await new UnitOfWork(Contexto).SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();
    }

    private async Task<Produto> SemearProduto(Guid subcategoriaId, ProdutoStatus status = ProdutoStatus.Ativo)
    {
        var produto = new Produto(subcategoriaId, "Brigadeiro", 5m, "https://exemplo.com/doce.png", 0.1m, 5m, 5m, 5m, status);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }

    private static Banner ParaSubcategoria(Guid subcategoriaId, string texto, short ordem = 0,
        DateOnly? inicio = null, DateOnly? fim = null) =>
        new(Imagem, texto, ordem, inicio ?? Hoje, fim, null, subcategoriaId, null);

    [Fact]
    public async Task Dado_BannerDeSubcategoria_Quando_ListarVigentes_Entao_DeveVirComSubcategoriaECategoria()
    {
        var subcategoriaId = await SemearSubcategoria();
        await Salvar(ParaSubcategoria(subcategoriaId, "Doces de tacho"));

        var banner = Assert.Single(await Repositorio.ListarVigentes(Hoje));

        Assert.Equal("Doces de Tacho", banner.Subcategoria?.Nome);
        Assert.Equal("Doces", banner.Subcategoria?.Categoria?.Nome);
    }

    [Fact]
    public async Task Dado_BannersForaDoPeriodoOuDesativados_Quando_ListarVigentes_Entao_DeveTrazerSoOsVigentes()
    {
        var subcategoriaId = await SemearSubcategoria();
        var desativado = ParaSubcategoria(subcategoriaId, "Desativado");
        desativado.Desativar();

        await Salvar(
            ParaSubcategoria(subcategoriaId, "Sem fim", inicio: Hoje.AddDays(-30)),
            ParaSubcategoria(subcategoriaId, "Termina hoje", inicio: Hoje.AddDays(-1), fim: Hoje),
            ParaSubcategoria(subcategoriaId, "Encerrado", inicio: Hoje.AddDays(-5), fim: Hoje.AddDays(-1)),
            ParaSubcategoria(subcategoriaId, "Agendado", inicio: Hoje.AddDays(1)),
            desativado);

        var vigentes = await Repositorio.ListarVigentes(Hoje);

        Assert.Equal(["Sem fim", "Termina hoje"], vigentes.Select(b => b.TextoAlternativo).Order());
    }

    [Fact]
    public async Task Dado_BannersComMesmaOrdem_Quando_ListarVigentes_Entao_DeveDesempatarPorDataInicio()
    {
        var subcategoriaId = await SemearSubcategoria();

        await Salvar(
            ParaSubcategoria(subcategoriaId, "Segundo", ordem: 1, inicio: Hoje),
            ParaSubcategoria(subcategoriaId, "Primeiro", ordem: 0, inicio: Hoje),
            ParaSubcategoria(subcategoriaId, "Mais antigo da ordem 1", ordem: 1, inicio: Hoje.AddDays(-3)));

        var vigentes = await Repositorio.ListarVigentes(Hoje);

        Assert.Equal(["Primeiro", "Mais antigo da ordem 1", "Segundo"], vigentes.Select(b => b.TextoAlternativo));
    }

    [Fact]
    public async Task Dado_ProdutoDestinoInativo_Quando_ListarVigentes_Entao_NaoDeveRetornarBanner()
    {
        var subcategoriaId = await SemearSubcategoria();
        var inativo = await SemearProduto(subcategoriaId, ProdutoStatus.Inativo);
        var foraDeEstoque = await SemearProduto(subcategoriaId, ProdutoStatus.ForaDeEstoque);

        await Salvar(
            new Banner(Imagem, "Produto inativo", 0, Hoje, null, null, null, inativo.ProdutoId),
            new Banner(Imagem, "Fora de estoque", 0, Hoje, null, null, null, foraDeEstoque.ProdutoId));

        var banner = Assert.Single(await Repositorio.ListarVigentes(Hoje));

        Assert.Equal("Fora de estoque", banner.TextoAlternativo);
        Assert.Equal(foraDeEstoque.ProdutoId, banner.Produto?.ProdutoId);
    }

    [Fact]
    public async Task Dado_BannersEmVariasSituacoes_Quando_ListarParaGestao_Entao_DeveTrazerTodosComDestino()
    {
        var subcategoriaId = await SemearSubcategoria();
        var categoriaId = (await Contexto.Subcategorias.AsNoTracking().SingleAsync()).CategoriaId;
        var encerrado = ParaSubcategoria(subcategoriaId, "Encerrado", inicio: Hoje.AddDays(-9), fim: Hoje.AddDays(-1));
        encerrado.Desativar();

        await Salvar(encerrado, new Banner(Imagem, "Categoria", 2, Hoje, null, categoriaId, null, null));

        var lista = await Repositorio.ListarParaGestao();

        Assert.Equal(2, lista.Count);
        Assert.Equal("Doces", lista.Single(b => b.TextoAlternativo == "Categoria").Categoria?.Nome);
        Assert.Equal("Doces de Tacho", lista.Single(b => b.TextoAlternativo == "Encerrado").Subcategoria?.Nome);
    }

    [Fact]
    public async Task Dado_BannerComDoisDestinos_Quando_Salvar_Entao_BancoDeveRecusar()
    {
        var subcategoriaId = await SemearSubcategoria();
        var categoriaId = (await Contexto.Subcategorias.AsNoTracking().SingleAsync()).CategoriaId;

        await Assert.ThrowsAsync<Npgsql.PostgresException>(() => Contexto.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "Banner" ("BannerId", "ImagemUrl", "TextoAlternativo", "Ordem", "DataInicio", "Ativo", "CategoriaId", "SubcategoriaId")
            VALUES ({Guid.NewGuid()}, {Imagem}, {"Dois destinos"}, {(short)0}, {Hoje}, {true}, {categoriaId}, {subcategoriaId})
            """));
    }
}
