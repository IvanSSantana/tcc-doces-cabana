using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class ProdutoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task DadoProdutoPersistido_QuandoBuscarPorId_EntaoRetornaProduto()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var produto = CriarProduto();

        await repositorio.Adicionar(produto);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var encontrado = await repositorio.BuscarPorId(produto.ProdutoId);

        Assert.NotNull(encontrado);
        Assert.Equal(produto.Nome, encontrado.Nome);
    }

    [Fact]
    public async Task DadoDoisProdutos_QuandoBuscarTodos_EntaoRetornaAmbos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);

        await repositorio.Adicionar(CriarProduto());
        await repositorio.Adicionar(CriarProduto());
        await unidadeDeTrabalho.SalvarAlteracoes();

        var produtos = (await repositorio.BuscarTodos()).ToList();

        Assert.Equal(2, produtos.Count);
    }

    [Fact]
    public async Task DadoProdutoRemovidoEmTransacao_QuandoBuscar_EntaoNaoEncontra()
    {
        IUnitOfWork unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var produto = CriarProduto();

        await unidadeDeTrabalho.ExecutarEmTransacao(async () =>
        {
            await repositorio.Adicionar(produto);
        });

        await unidadeDeTrabalho.ExecutarEmTransacao(async () =>
        {
            repositorio.Remover(produto);
        });

        var encontrado = await repositorio.BuscarPorId(produto.ProdutoId);

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task DadoFalhaEmTransacao_QuandoReverter_EntaoProdutoNaoPersistido()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var produto = CriarProduto();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unidadeDeTrabalho.ExecutarEmTransacao(async () =>
            {
                await repositorio.Adicionar(produto);
                throw new InvalidOperationException("Falha simulada");
            }));

        var encontrado = await Contexto.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);

        Assert.Null(encontrado);
    }

    private static Produto CriarProduto() =>
        new(Guid.NewGuid(), "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");
}
