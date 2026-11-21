using DocesCabana.Tests.Integration.Infraestrutura;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class CupomRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
{
    private static readonly DateOnly Hoje = new(2026, 11, 20);

    [Fact]
    public async Task Dado_CodigoMinusculo_Quando_BuscarPorCodigo_Entao_DeveEncontrar()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new CupomRepository(Contexto);
        await repositorio.Adicionar(CriarCupom("BEMVINDO10"));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.BuscarPorCodigo(Cupom.NormalizarCodigo(" bemvindo10 "));

        Assert.NotNull(encontrado);
        Assert.Equal("BEMVINDO10", encontrado.Codigo);
        Assert.Equal(TipoDeDesconto.Percentual, encontrado.Tipo);
    }

    [Fact]
    public async Task Dado_CupomDeCategoria_Quando_BuscarPorCodigo_Entao_DeveTrazerACategoria()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new CupomRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var categoriaId = (await Contexto.Subcategorias.FindAsync(subcategoriaId))!.CategoriaId;
        await repositorio.Adicionar(CriarCupom("DOCES15", categoriaId: categoriaId));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.BuscarPorCodigo("DOCES15");

        Assert.Equal("Doces", encontrado!.Categoria?.Nome);
    }

    [Fact]
    public async Task Dado_CodigoInexistente_Quando_BuscarPorCodigo_Entao_DeveRetornarNulo()
    {
        var repositorio = new CupomRepository(Contexto);

        Assert.Null(await repositorio.BuscarPorCodigo("NAOEXISTE"));
    }

    [Fact]
    public async Task Dado_CupomCadastrado_Quando_ExisteCodigo_Entao_DeveSerVerdadeiroSoParaEle()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new CupomRepository(Contexto);
        await repositorio.Adicionar(CriarCupom("BEMVINDO10"));
        await unidadeDeTrabalho.SalvarAlteracoes();

        Assert.True(await repositorio.ExisteCodigo("BEMVINDO10"));
        Assert.False(await repositorio.ExisteCodigo("OUTRO10"));
    }

    [Fact]
    public async Task Dado_DoisCuponsComMesmoCodigo_Quando_Salvar_Entao_DeveFalhar()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new CupomRepository(Contexto);
        await repositorio.Adicionar(CriarCupom("BEMVINDO10"));
        await unidadeDeTrabalho.SalvarAlteracoes();

        await repositorio.Adicionar(CriarCupom("bemvindo10"));

        await Assert.ThrowsAsync<DbUpdateException>(() => unidadeDeTrabalho.SalvarAlteracoes());
    }

    [Fact]
    public async Task Dado_CuponsCadastrados_Quando_ListarParaGestao_Entao_DeveTrazerTodosComSubcategoria()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new CupomRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var desativado = CriarCupom("ANTIGO5");
        desativado.Desativar();
        await repositorio.Adicionar(desativado);
        await repositorio.Adicionar(CriarCupom("TACHO20", subcategoriaId: subcategoriaId));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var lista = await repositorio.ListarParaGestao();

        Assert.Equal(2, lista.Count);
        Assert.Equal("Doces de Tacho", lista.Single(c => c.Codigo == "TACHO20").Subcategoria?.Nome);
    }

    [Fact]
    public async Task Dado_BancoSemeadoDuasVezes_Quando_SemearCupomDeDemonstracao_Entao_DeveHaverUmBemVindo10ValendoUmAno()
    {
        await DocesCabana.Infrastructure.DatabaseContext.Seed.DbInitializer.SemearCupomDeDemonstracao(Contexto, Hoje);
        await DocesCabana.Infrastructure.DatabaseContext.Seed.DbInitializer.SemearCupomDeDemonstracao(Contexto, Hoje);
        Contexto.ChangeTracker.Clear();

        var cupom = Assert.Single(await new CupomRepository(Contexto).ListarParaGestao());

        Assert.Equal("BEMVINDO10", cupom.Codigo);
        Assert.Equal(10, cupom.Valor);
        Assert.True(cupom.UmUsoPorCliente);
        Assert.Null(cupom.ValorMinimo);
        Assert.Equal(Hoje, cupom.DataInicio);
        Assert.Equal(Hoje.AddYears(1), cupom.DataFim);
    }

    private static Cupom CriarCupom(string codigo, Guid? categoriaId = null, Guid? subcategoriaId = null) =>
        new(codigo, TipoDeDesconto.Percentual, 10, null, null, Hoje, Hoje.AddDays(30),
            null, true, categoriaId, subcategoriaId);
}
