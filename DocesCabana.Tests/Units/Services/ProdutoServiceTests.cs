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
    private readonly Mock<IFavoritoRepository> _favoritoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProdutoService _produtoService;

    public ProdutoServiceTests()
    {
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _avaliacaoServiceMock = new Mock<IAvaliacaoService>();
        _favoritoRepositoryMock = new Mock<IFavoritoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _produtoService = new ProdutoService(
            _produtoRepositoryMock.Object, _avaliacaoServiceMock.Object, _favoritoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Dado_ProdutosCadastrados_Quando_BuscarTodosProdutos_Entao_DeveRetornarListaDeProdutos()
    {
        var produtos = new List<Produto>
        {
            new Produto(Guid.NewGuid(), "Bolo de Chocolate", 15.00m, "https://imagem.com/bolo.jpg", 0.5m, 10m, 15m, 20m),
            new Produto(Guid.NewGuid(), "Doce de Leite", 8.50m, "https://imagem.com/doce.jpg", 0.5m, 10m, 15m, 20m)
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
    public async Task Dado_ProdutoInativo_Quando_BuscarTodosProdutos_Entao_NaoDeveIncluiLo()
    {
        var produtos = new List<Produto>
        {
            new Produto(Guid.NewGuid(), "Bolo Ativo", 15.00m, "https://imagem.com/bolo.jpg", 0.5m, 10m, 15m, 20m, ProdutoStatus.Ativo),
            new Produto(Guid.NewGuid(), "Doce Inativo", 8.50m, "https://imagem.com/doce.jpg", 0.5m, 10m, 15m, 20m, ProdutoStatus.Inativo),
            new Produto(Guid.NewGuid(), "Bala Fora de Estoque", 5.00m, "https://imagem.com/bala.jpg", 0.5m, 10m, 15m, 20m, ProdutoStatus.ForaDeEstoque),
        };

        _produtoRepositoryMock.Setup(r => r.BuscarTodos())
            .ReturnsAsync(produtos);

        var resultado = await _produtoService.BuscarTodosProdutos();

        Assert.Equal(2, resultado.Count);
        Assert.DoesNotContain(resultado, p => p.Nome == "Doce Inativo");
        Assert.Contains(resultado, p => p.Nome == "Bolo Ativo");
        Assert.Contains(resultado, p => p.Nome == "Bala Fora de Estoque");
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
        var produto = new Produto(Guid.NewGuid(), "Bolo de Chocolate", 15.00m, "https://imagem.com/bolo.jpg", 0.5m, 10m, 15m, 20m, id: idEsperado);

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
            SubcategoriaId = Guid.NewGuid(),
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
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
            SubcategoriaId = Guid.NewGuid(),
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
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
            SubcategoriaId = Guid.NewGuid(),
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
        };

        var resultado = await _produtoService.Cadastrar(dto);

        Assert.Equal(ProdutoStatus.Inativo, resultado.Status);
        Assert.NotEqual(Guid.Empty, resultado.ProdutoId);
    }

    [Fact]
    public async Task Dado_ProdutoAtivo_Quando_BuscarDetalhe_Entao_DeveTrazerNomePrecoEResumo()
    {
        var produtoId = Guid.NewGuid();
        var produto = new Produto(Guid.NewGuid(), "Pé de Moleque Doce de Matar", 29.99m,
            "https://imagem.com/pe-de-moleque.jpg", 0.5m, 10m, 15m, 20m, id: produtoId, descricao: "Feito com amendoim torrado na hora.");

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
        var idInexistente = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarDetalhePorId(idInexistente)).ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _produtoService.BuscarDetalhe(idInexistente, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null));
    }

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();
        var produto = new Produto(Guid.NewGuid(), "Bolo de Teste", 10.00m,
            "https://imagem.com/bolo.jpg", 0.5m, 10m, 15m, 20m, ProdutoStatus.Inativo, produtoId);

        _produtoRepositoryMock.Setup(r => r.BuscarDetalhePorId(produtoId)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _produtoService.BuscarDetalhe(produtoId, OrdenacaoAvaliacao.Relevantes, 5, usuarioAtual: null));
    }

    [Fact]
    public async Task Dado_UmLimite_Quando_BuscarDestaquesDaVitrine_Entao_DevePedirAoRepositorioExatamenteEsseLimite()
    {
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(
                It.Is<FiltroCatalogoDTO>(f =>
                    f.CategoriaId == null &&
                    f.SubcategoriaIds.Count == 0 &&
                    !f.ApenasSemAcucar &&
                    f.Ordenacao == OrdenacaoCatalogo.MaisVendidos &&
                    f.TermoNormalizado == null),
                pagina: 1, tamanhoDaPagina: 8))
            .ReturnsAsync([]);

        await _produtoService.BuscarDestaquesDaVitrine(8);

        _produtoRepositoryMock.Verify(r => r.BuscarPaginaDoCatalogo(
            It.IsAny<FiltroCatalogoDTO>(), 1, 8), Times.Once);
    }

    [Fact]
    public async Task Dado_VisitanteSemAutenticacao_Quando_BuscarDestaquesDaVitrine_Entao_NaoDeveConsultarFavoritos()
    {
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), 1, 8))
            .ReturnsAsync([]);

        await _produtoService.BuscarDestaquesDaVitrine(8, usuarioId: null);

        _favoritoRepositoryMock.Verify(
            r => r.IdsPorUsuario(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_BuscarDestaquesDaVitrine_Entao_DeveMarcarOsFavoritados()
    {
        var usuarioId = Guid.NewGuid();
        var produtoFavoritado = new Produto(Guid.NewGuid(), "Bolo Favorito", 15.00m, "https://imagem.com/bolo.jpg", 0.5m, 10m, 15m, 20m);
        var produtoNaoFavoritado = new Produto(Guid.NewGuid(), "Doce Comum", 8.50m, "https://imagem.com/doce.jpg", 0.5m, 10m, 15m, 20m);

        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), 1, 8))
            .ReturnsAsync([produtoFavoritado, produtoNaoFavoritado]);
        _favoritoRepositoryMock
            .Setup(r => r.IdsPorUsuario(usuarioId, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([produtoFavoritado.ProdutoId]);

        var resultado = await _produtoService.BuscarDestaquesDaVitrine(8, usuarioId);

        Assert.True(resultado.Single(p => p.ProdutoId == produtoFavoritado.ProdutoId).EstaFavorito);
        Assert.False(resultado.Single(p => p.ProdutoId == produtoNaoFavoritado.ProdutoId).EstaFavorito);
    }
}
