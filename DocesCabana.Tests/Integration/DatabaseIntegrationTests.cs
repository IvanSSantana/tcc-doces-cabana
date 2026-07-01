using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration;

public class DatabaseIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarAoRepositorioSemSalvar_Entao_NaoDeveEstarNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Null(produtoNoBanco);
    }

    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarECommitarPeloUnitOfWork_Entao_DevePersistirNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var uow = new UnitOfWork(Contexto);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);
        var salvos = await uow.SalvarAlteracoes();

        Assert.True(salvos > 0);
        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.NotNull(produtoNoBanco);
        Assert.Equal("Bolo de Cenoura", produtoNoBanco.Nome);
    }

    [Fact]
    public async Task Dado_UmaTransacaoAtiva_Quando_InserirERealizarRollback_Entao_NaoDeveSalvarNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var uow = new UnitOfWork(Contexto);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Trufa de Chocolate", 4.50m, "https://imagem.com/trufa.jpg");

        await using (var transacao = await uow.IniciarTransacao())
        {
            await repositorio.Adicionar(produto);
            await uow.SalvarAlteracoes();
            await transacao.Reverter();
        }

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Null(produtoNoBanco);
    }

    [Fact]
    public async Task Dado_UmaTransacaoAtiva_Quando_InserirEConfirmarTransacao_Entao_DevePersistirNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var uow = new UnitOfWork(Contexto);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Trufa de Chocolate", 4.50m, "https://imagem.com/trufa.jpg");

        await using (var transacao = await uow.IniciarTransacao())
        {
            await repositorio.Adicionar(produto);
            await uow.SalvarAlteracoes();
            await transacao.Confirmar();
        }

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.NotNull(produtoNoBanco);
        Assert.Equal("Trufa de Chocolate", produtoNoBanco.Nome);
    }
}
