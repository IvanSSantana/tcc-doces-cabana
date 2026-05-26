using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DocesCabanaDbContext> _options;

    public DatabaseIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<DocesCabanaDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new DocesCabanaDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarAoRepositorioSemSalvar_Entao_NaoDeveEstarNoBanco()
    {
        using var context = new DocesCabanaDbContext(_options);
        var repositorio = new Repository<Produto>(context);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);

        using var contextLeitura = new DocesCabanaDbContext(_options);
        var produtoNoBanco = await contextLeitura.Produtos.FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Null(produtoNoBanco);
    }

    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarECommitarPeloUnitOfWork_Entao_DevePersistirNoBanco()
    {
        using var context = new DocesCabanaDbContext(_options);
        var repositorio = new Repository<Produto>(context);
        var uow = new UnitOfWork(context);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);
        var salvos = await uow.SalvarAlteracoes();

        Assert.True(salvos > 0);
        using var contextLeitura = new DocesCabanaDbContext(_options);
        var produtoNoBanco = await contextLeitura.Produtos.FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.NotNull(produtoNoBanco);
        Assert.Equal("Bolo de Cenoura", produtoNoBanco.Nome);
    }

    [Fact]
    public async Task Dado_UmaTransacaoAtiva_Quando_InserirERealizarRollback_Entao_NaoDeveSalvarNoBanco()
    {
        using var context = new DocesCabanaDbContext(_options);
        var repositorio = new Repository<Produto>(context);
        var uow = new UnitOfWork(context);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Trufa de Chocolate", 4.50m, "https://imagem.com/trufa.jpg");

        await using (var transacao = await uow.IniciarTransacao())
        {
            await repositorio.Adicionar(produto);
            await uow.SalvarAlteracoes();
            await transacao.Reverter();
        }

        using var contextLeitura = new DocesCabanaDbContext(_options);
        var produtoNoBanco = await contextLeitura.Produtos.FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Null(produtoNoBanco);
    }

    [Fact]
    public async Task Dado_UmaTransacaoAtiva_Quando_InserirEConfirmarTransacao_Entao_DevePersistirNoBanco()
    {
        using var context = new DocesCabanaDbContext(_options);
        var repositorio = new Repository<Produto>(context);
        var uow = new UnitOfWork(context);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Trufa de Chocolate", 4.50m, "https://imagem.com/trufa.jpg");

        await using (var transacao = await uow.IniciarTransacao())
        {
            await repositorio.Adicionar(produto);
            await uow.SalvarAlteracoes();
            await transacao.Confirmar();
        }

        using var contextLeitura = new DocesCabanaDbContext(_options);
        var produtoNoBanco = await contextLeitura.Produtos.FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.NotNull(produtoNoBanco);
        Assert.Equal("Trufa de Chocolate", produtoNoBanco.Nome);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
