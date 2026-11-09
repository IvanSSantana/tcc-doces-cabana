using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.Tests.Integration.Infraestrutura;

namespace DocesCabana.Tests.Integration.Repositories;

public class ProdutoGestaoRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
{
    [Fact]
    public async Task Dado_ProdutosDeCadaStatus_Quando_BuscarPaginaDaGestaoComFiltroTodos_Entao_NaoDeveTrazerInativos()
    {
        var subId = await SemearSubcategoria();
        await SemearProduto(subId, "Ativo", ProdutoStatus.Ativo);
        await SemearProduto(subId, "Sem Estoque", ProdutoStatus.ForaDeEstoque);
        await SemearProduto(subId, "Inativo", ProdutoStatus.Inativo);
        var repositorio = new ProdutoRepository(Contexto);

        var pagina = await repositorio.BuscarPaginaDaGestao(FiltroDeGestaoDeProduto.Todos, 1, 10);
        var total = await repositorio.ContarNaGestao(FiltroDeGestaoDeProduto.Todos);

        Assert.Equal(2, pagina.Count);
        Assert.DoesNotContain(pagina, p => p.Status == ProdutoStatus.Inativo);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task Dado_ProdutosDeCadaStatus_Quando_BuscarPaginaDaGestaoComFiltroInativos_Entao_DeveTrazerSomenteInativos()
    {
        var subId = await SemearSubcategoria();
        await SemearProduto(subId, "Ativo", ProdutoStatus.Ativo);
        await SemearProduto(subId, "Sem Estoque", ProdutoStatus.ForaDeEstoque);
        await SemearProduto(subId, "Inativo", ProdutoStatus.Inativo);
        var repositorio = new ProdutoRepository(Contexto);

        var pagina = await repositorio.BuscarPaginaDaGestao(FiltroDeGestaoDeProduto.Inativos, 1, 10);
        var total = await repositorio.ContarNaGestao(FiltroDeGestaoDeProduto.Inativos);

        var unico = Assert.Single(pagina);
        Assert.Equal("Inativo", unico.Nome);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task Dado_VinteEUmProdutos_Quando_BuscarTerceiraPagina_Entao_DeveTrazerUmEmOrdemDeNome()
    {
        var subId = await SemearSubcategoria();
        for (var i = 1; i <= 21; i++)
            await SemearProduto(subId, $"Produto {i:00}", ProdutoStatus.Ativo);
        var repositorio = new ProdutoRepository(Contexto);

        var primeira = await repositorio.BuscarPaginaDaGestao(FiltroDeGestaoDeProduto.Todos, 1, 10);
        var terceira = await repositorio.BuscarPaginaDaGestao(FiltroDeGestaoDeProduto.Todos, 3, 10);
        var total = await repositorio.ContarNaGestao(FiltroDeGestaoDeProduto.Todos);

        Assert.Equal("Produto 01", primeira[0].Nome);
        Assert.Equal(10, primeira.Count);
        Assert.Equal("Produto 21", Assert.Single(terceira).Nome);
        Assert.Equal(21, total);
    }

    [Fact]
    public async Task Dado_ProdutoPersistido_Quando_AtualizarPrecoEStatus_Entao_DeveLerOsValoresNovos()
    {
        var subId = await SemearSubcategoria();
        var produto = await SemearProduto(subId, "Bolo", ProdutoStatus.Ativo);
        var repositorio = new ProdutoRepository(Contexto);
        var servico = new ProdutoService(
            repositorio, new Mock<IAvaliacaoService>().Object, new Mock<DocesCabana.Application.Contracts.Repositories.IFavoritoRepository>().Object,
            new UnitOfWork(Contexto));
        var dto = new ProdutoDTO
        {
            ProdutoId = produto.ProdutoId, Nome = "Bolo Editado", Preco = 19.9m, Status = ProdutoStatus.ForaDeEstoque,
            ImagemUrl = produto.ImagemUrl, SubcategoriaId = subId, Peso = 0.5m, Altura = 10m, Largura = 15m, Comprimento = 20m
        };

        await servico.Atualizar(dto);

        Contexto.ChangeTracker.Clear();
        var lido = await Contexto.Produtos.AsNoTracking().SingleAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Equal("Bolo Editado", lido.Nome);
        Assert.Equal("bolo editado", lido.NomeNormalizado);
        Assert.Equal(19.9m, lido.Preco);
        Assert.Equal(ProdutoStatus.ForaDeEstoque, lido.Status);
    }

    private async Task<Produto> SemearProduto(Guid subcategoriaId, string nome, ProdutoStatus status)
    {
        var produto = new Produto(subcategoriaId, nome, 10m, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m, status);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }
}
