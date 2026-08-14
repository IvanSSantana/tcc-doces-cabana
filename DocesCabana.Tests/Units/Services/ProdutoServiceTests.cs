using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly Mock<IAvaliacaoService> _avaliacaoServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProdutoService _produtoService;

    public ProdutoServiceTests()
    {
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _avaliacaoServiceMock = new Mock<IAvaliacaoService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _produtoService = new ProdutoService(_produtoRepositoryMock.Object, _avaliacaoServiceMock.Object, _unitOfWorkMock.Object);
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
    public async Task Dado_ProdutoValido_Quando_Cadastrar_Entao_DeveChamarSalvarAlteracoes()
    {
        var dto = new ProdutoDTO
        {
            Nome = "Brigadeiro Gourmet",
            Preco = 5.50m,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = "https://imagem.com/brigadeiro.jpg",
            SubcategoriaId = Guid.NewGuid()
        };

        await _produtoService.Cadastrar(dto);

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
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

    [Fact]
    public async Task Dado_ProdutoAtivo_Quando_BuscarDetalhe_Entao_DeveTrazerNomePrecoEResumo()
    {
        // CA-01
        var produtoId = Guid.NewGuid();
        var produto = new Produto(Guid.NewGuid(), "Pé de Moleque Doce de Matar", 29.99m,
            "https://imagem.com/pe-de-moleque.jpg", id: produtoId, descricao: "Feito com amendoim torrado na hora.");

        _produtoRepositoryMock.Setup(r => r.BuscarDetalhePorId(produtoId)).ReturnsAsync(produto);
        _avaliacaoServiceMock.Setup(s => s.ResumirPorProduto(produtoId))
            .ReturnsAsync(new ResumoAvaliacoesDTO { Media = null, Total = 0, Distribuicao = new Dictionary<byte, int>() });
        _avaliacaoServiceMock
            .Setup(s => s.ListarPorProduto(produtoId, OrdenacaoAvaliacao.Relevantes, 5, null))
            .ReturnsAsync(new PaginaAvaliacoesDTO { Itens = [], Ordenacao = OrdenacaoAvaliacao.Relevantes, Exibindo = 0, Total = 0, TemMais = false });

        var detalhe = await _produtoService.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null);

        Assert.Equal("Pé de Moleque Doce de Matar", detalhe.Nome);
        Assert.Equal(29.99m, detalhe.Preco);
        Assert.Equal("Feito com amendoim torrado na hora.", detalhe.Resumo);
    }

    [Fact]
    public async Task Dado_IdInexistente_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException()
    {
        // CA-04
        var idInexistente = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarDetalhePorId(idInexistente)).ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _produtoService.BuscarDetalhe(idInexistente, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null));
    }

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException()
    {
        // CA-05, RN-12: produto inativo não é visível ao cliente por nenhum caminho.
        var produtoId = Guid.NewGuid();
        var produto = new Produto(Guid.NewGuid(), "Bolo de Teste", 10.00m,
            "https://imagem.com/bolo.jpg", ProdutoStatus.Inativo, produtoId);

        _produtoRepositoryMock.Setup(r => r.BuscarDetalhePorId(produtoId)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _produtoService.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null));
    }
}
