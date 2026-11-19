using DocesCabana.Tests.Integration.Infraestrutura;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class PromocaoRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
{
    private static readonly DateOnly Hoje = new(2026, 11, 20);

    [Fact]
    public async Task Dado_PromocaoPorProdutos_Quando_Salvar_Entao_DeveVoltarComProdutos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PromocaoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var brigadeiro = await SemearProduto(subcategoriaId, "Brigadeiro");
        var beijinho = await SemearProduto(subcategoriaId, "Beijinho");

        var promocao = new Promocao("Leve 3", TipoDeDesconto.CompreXLeveY, 0, 3, 2,
            Hoje, Hoje.AddDays(5), null, null, [brigadeiro, beijinho]);
        await repositorio.Adicionar(promocao);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrada = await repositorio.BuscarComProdutos(promocao.PromocaoId);

        Assert.NotNull(encontrada);
        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, encontrada.Abrangencia);
        Assert.Equal(2, encontrada.Produtos.Count);
        Assert.Equal((short)3, encontrada.QuantidadeLeve);
        Assert.Equal(Hoje, encontrada.DataInicio);
    }

    [Fact]
    public async Task Dado_PromocaoPorProdutos_Quando_TrocarProdutosESalvar_Entao_DeveGuardarSoOsNovos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PromocaoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var brigadeiro = await SemearProduto(subcategoriaId, "Brigadeiro");
        var beijinho = await SemearProduto(subcategoriaId, "Beijinho");

        var promocao = new Promocao("Dez", TipoDeDesconto.Percentual, 10, null, null,
            Hoje, Hoje, null, null, [brigadeiro]);
        await repositorio.Adicionar(promocao);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var paraEditar = await repositorio.BuscarComProdutos(promocao.PromocaoId);
        var novos = await new ProdutoRepository(Contexto).BuscarPorIds([beijinho.ProdutoId]);
        paraEditar!.AlterarAbrangencia(null, null, novos);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrada = await repositorio.BuscarComProdutos(promocao.PromocaoId);

        Assert.Equal(beijinho.ProdutoId, Assert.Single(encontrada!.Produtos).ProdutoId);
    }

    [Fact]
    public async Task Dado_PromocoesEmVariosPeriodos_Quando_ListarVigentes_Entao_DeveTrazerSoAsDeHoje()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PromocaoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();

        var encerrada = CriarPorSubcategoria("Encerrada", subcategoriaId, Hoje.AddDays(-5), Hoje.AddDays(-1));
        var terminaHoje = CriarPorSubcategoria("Termina hoje", subcategoriaId, Hoje.AddDays(-5), Hoje);
        var comecaHoje = CriarPorSubcategoria("Começa hoje", subcategoriaId, Hoje, Hoje.AddDays(5));
        var agendada = CriarPorSubcategoria("Agendada", subcategoriaId, Hoje.AddDays(1), Hoje.AddDays(5));
        var desativada = CriarPorSubcategoria("Desativada", subcategoriaId, Hoje, Hoje);
        desativada.Desativar();

        foreach (var promocao in new[] { encerrada, terminaHoje, comecaHoje, agendada, desativada })
            await repositorio.Adicionar(promocao);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var vigentes = await repositorio.ListarVigentes(Hoje);

        Assert.Equal(
            new[] { "Começa hoje", "Termina hoje" },
            vigentes.Select(p => p.Nome).Order().ToArray());
    }

    [Fact]
    public async Task Dado_PromocaoPorProdutos_Quando_ListarVigentes_Entao_DeveIncluirOsProdutos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PromocaoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var brigadeiro = await SemearProduto(subcategoriaId, "Brigadeiro");

        await repositorio.Adicionar(new Promocao("Dez", TipoDeDesconto.Percentual, 10, null, null,
            Hoje, Hoje, null, null, [brigadeiro]));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var vigente = Assert.Single(await repositorio.ListarVigentes(Hoje));

        Assert.True(vigente.Alcanca(brigadeiro.ProdutoId, subcategoriaId, Guid.NewGuid()));
    }

    [Fact]
    public async Task Dado_PromocoesCadastradas_Quando_ListarParaGestao_Entao_DeveTrazerTodasComCategoria()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PromocaoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var categoriaId = (await Contexto.Subcategorias.FindAsync(subcategoriaId))!.CategoriaId;

        var porCategoria = new Promocao("Categoria", TipoDeDesconto.ValorFixo, 2, null, null,
            Hoje, Hoje, categoriaId, null, null);
        var desativada = CriarPorSubcategoria("Desativada", subcategoriaId, Hoje, Hoje);
        desativada.Desativar();
        await repositorio.Adicionar(porCategoria);
        await repositorio.Adicionar(desativada);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var lista = await repositorio.ListarParaGestao();

        Assert.Equal(2, lista.Count);
        Assert.Equal("Doces", lista.Single(p => p.Nome == "Categoria").Categoria?.Nome);
        Assert.Equal("Doces de Tacho", lista.Single(p => p.Nome == "Desativada").Subcategoria?.Nome);
    }

    [Fact]
    public async Task Dado_Ids_Quando_BuscarPorIds_Entao_DeveTrazerSoOsProdutosPedidos()
    {
        var repositorio = new ProdutoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var brigadeiro = await SemearProduto(subcategoriaId, "Brigadeiro");
        await SemearProduto(subcategoriaId, "Beijinho");

        var encontrados = await repositorio.BuscarPorIds([brigadeiro.ProdutoId, Guid.NewGuid()]);

        Assert.Equal(brigadeiro.ProdutoId, Assert.Single(encontrados).ProdutoId);
    }

    private static Promocao CriarPorSubcategoria(string nome, Guid subcategoriaId, DateOnly inicio, DateOnly fim) =>
        new(nome, TipoDeDesconto.Percentual, 10, null, null, inicio, fim, null, subcategoriaId, null);

    private async Task<Produto> SemearProduto(Guid subcategoriaId, string nome)
    {
        var produto = new Produto(subcategoriaId, nome, 5m, "https://exemplo.com/doce.png", 0.1m, 5m, 5m, 5m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }
}
