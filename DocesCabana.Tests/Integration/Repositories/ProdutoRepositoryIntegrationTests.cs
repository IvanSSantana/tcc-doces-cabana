using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class ProdutoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_ProdutoPersistido_QuandoBuscarPorId_Entao_DeveRetornarProduto()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var produto = await CriarProduto();

        await repositorio.Adicionar(produto);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var encontrado = await repositorio.BuscarPorId(produto.ProdutoId);

        Assert.NotNull(encontrado);
        Assert.Equal(produto.Nome, encontrado.Nome);
    }

    [Fact]
    public async Task Dado_DoisProdutos_Quando_BuscarTodos_Entao_DeveRetornarAmbos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var subcategoriaId = await SemearSubcategoria();

        await repositorio.Adicionar(CriarProduto(subcategoriaId));
        await repositorio.Adicionar(CriarProduto(subcategoriaId));
        await unidadeDeTrabalho.SalvarAlteracoes();

        var produtos = (await repositorio.BuscarTodos()).ToList();

        Assert.Equal(2, produtos.Count);
    }

    [Fact]
    public async Task Dado_ProdutoPersistido_Quando_RemoverESalvar_Entao_NaoDeveEncontrar()
    {
        IUnitOfWork unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new ProdutoRepository(Contexto);
        var produto = await CriarProduto();

        await repositorio.Adicionar(produto);
        await unidadeDeTrabalho.SalvarAlteracoes();

        repositorio.Remover(produto);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var encontrado = await repositorio.BuscarPorId(produto.ProdutoId);

        Assert.Null(encontrado);
    }

    private async Task<Produto> CriarProduto()
    {
        var subcategoriaId = await SemearSubcategoria();
        return CriarProduto(subcategoriaId);
    }

    private static Produto CriarProduto(Guid subcategoriaId) =>
        new(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg");
}
