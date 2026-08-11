using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly ProdutoService _produtoService;

    public ProdutoServiceTests()
    {
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _produtoService = new ProdutoService(_produtoRepositoryMock.Object);
    }

    [Fact]
    public async Task Dado_ProdutosCadastrados_Quando_BuscarTodosProdutos_Entao_DeveRetornarListaDeProdutos()
    {
        var produtos = new List<Produto>
        {
            new Produto(Guid.NewGuid(), "Bolo de Chocolate", 15.00m, "https://imagem.com/bolo.jpg"),
            new Produto(Guid.NewGuid(), "Doce de Leite", 8.50m, "https://imagem.com/doce.jpg")
        };

        _produtoRepositoryMock.Setup(r => r.BuscarTodos())
            .ReturnsAsync(produtos);

        var resultado = await _produtoService.BuscarTodosProdutos();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, p => p.Nome == "Bolo de Chocolate");
        Assert.Contains(resultado, p => p.Nome == "Doce de Leite");
    }

    [Fact]
    public async Task Dado_IdInexistente_Quando_BuscarProdutoPorId_Entao_DeveLancarKeyNotFoundException()
    {
        var idInexistente = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(idInexistente))
            .ReturnsAsync((Produto?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _produtoService.BuscarProdutoPorId(idInexistente));
    }

    [Fact]
    public async Task Dado_IdExistente_Quando_BuscarProdutoPorId_Entao_DeveRetornarProdutoDto()
    {
        var idEsperado = Guid.NewGuid();
        var produto = new Produto(Guid.NewGuid(), "Bolo de Chocolate", 15.00m, "https://imagem.com/bolo.jpg", id: idEsperado);

        _produtoRepositoryMock.Setup(r => r.BuscarPorId(idEsperado))
            .ReturnsAsync(produto);

        var resultado = await _produtoService.BuscarProdutoPorId(idEsperado);

        Assert.NotNull(resultado);
        Assert.Equal(idEsperado, resultado.ProdutoId);
        Assert.Equal("Bolo de Chocolate", resultado.Nome);
    }

    [Fact]
    public async Task Dado_ProdutoValido_Quando_Cadastrar_Entao_DeveAdicionarNoRepositorio()
    {
        var dto = new ProdutoDTO
        {
            Nome = "Brigadeiro Gourmet",
            Preco = 5.50m,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = "https://imagem.com/brigadeiro.jpg",
            SubcategoriaId = Guid.NewGuid()
        };

        var resultado = await _produtoService.Cadastrar(dto);

        _produtoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Produto>()), Times.Once);
        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Dado_ProdutoComStatusInativo_Quando_Cadastrar_Entao_DeveDevolverStatusEfetivoDaEntidade()
    {
        var dto = new ProdutoDTO
        {
            Nome = "Pé de Moça",
            Preco = 27.00m,
            Status = ProdutoStatus.Inativo,
            ImagemUrl = "https://imagem.com/pe-de-moca.jpg",
            SubcategoriaId = Guid.NewGuid()
        };

        var resultado = await _produtoService.Cadastrar(dto);

        Assert.Equal(ProdutoStatus.Inativo, resultado.Status);
        Assert.NotEqual(Guid.Empty, resultado.ProdutoId);
    }
}
