using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class FavoritoServiceTests
{
    private readonly Mock<IFavoritoRepository> _favoritoRepositoryMock;
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FavoritoService _favoritoService;

    public FavoritoServiceTests()
    {
        _favoritoRepositoryMock = new Mock<IFavoritoRepository>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _favoritoService = new FavoritoService(
            _favoritoRepositoryMock.Object,
            _produtoRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Produto CriarProduto(ProdutoStatus status = ProdutoStatus.Ativo) =>
        new(Guid.NewGuid(), "Brigadeiro", 5m, "https://imagem.com/brigadeiro.jpg", status);

    [Fact]
    public async Task Dado_ProdutoNaoFavoritado_Quando_Alternar_Entao_DeveFavoritarEDevolverTrue()
    {
        var produto = CriarProduto();
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _favoritoRepositoryMock.Setup(r => r.Buscar(produto.ProdutoId, usuarioId)).ReturnsAsync((Favorito?)null);

        var resultado = await _favoritoService.Alternar(produto.ProdutoId, usuarioId);

        Assert.True(resultado);
        _favoritoRepositoryMock.Verify(r => r.Adicionar(It.Is<Favorito>(f => f.ProdutoId == produto.ProdutoId && f.UsuarioId == usuarioId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ProdutoJaFavoritado_Quando_Alternar_Entao_DeveDesfavoritarEDevolverFalse()
    {
        var produto = CriarProduto();
        var usuarioId = Guid.NewGuid();
        var favoritoExistente = new Favorito(produto.ProdutoId, usuarioId);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _favoritoRepositoryMock.Setup(r => r.Buscar(produto.ProdutoId, usuarioId)).ReturnsAsync(favoritoExistente);

        var resultado = await _favoritoService.Alternar(produto.ProdutoId, usuarioId);

        Assert.False(resultado);
        _favoritoRepositoryMock.Verify(r => r.Remover(favoritoExistente), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ProdutoInexistente_Quando_Alternar_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoId)).ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _favoritoService.Alternar(produtoId, usuarioId));

        _favoritoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Favorito>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_Alternar_Entao_DeveLancarKeyNotFoundException()
    {
        // RN-01 da 012: produto inativo não existe do lado de fora — favoritar
        // um deles é o mesmo defeito de favoritar um produto que não existe.
        var produto = CriarProduto(ProdutoStatus.Inativo);
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _favoritoService.Alternar(produto.ProdutoId, usuarioId));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    [Fact]
    public async Task Dado_ProdutoForaDeEstoque_Quando_Alternar_Entao_DeveFavoritarNormalmente()
    {
        // Fora de estoque continua listado, sinalizado (RF-10 da 014) — não é
        // a mesma coisa que inativo, e continua favoritável.
        var produto = CriarProduto(ProdutoStatus.ForaDeEstoque);
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _favoritoRepositoryMock.Setup(r => r.Buscar(produto.ProdutoId, usuarioId)).ReturnsAsync((Favorito?)null);

        var resultado = await _favoritoService.Alternar(produto.ProdutoId, usuarioId);

        Assert.True(resultado);
    }

    [Fact]
    public async Task Dado_ProdutosFavoritadosDeUmUsuario_Quando_ListarDoUsuario_Entao_DeveDevolverSoOsDisponiveis()
    {
        var usuarioId = Guid.NewGuid();
        var produtoAtivo = CriarProduto();
        var produtoInativo = CriarProduto(ProdutoStatus.Inativo);
        var favoritoAtivo = new Favorito(produtoAtivo.ProdutoId, usuarioId);
        var favoritoInativo = new Favorito(produtoInativo.ProdutoId, usuarioId);

        typeof(Favorito).GetProperty(nameof(Favorito.Produto))!.SetValue(favoritoAtivo, produtoAtivo);
        typeof(Favorito).GetProperty(nameof(Favorito.Produto))!.SetValue(favoritoInativo, produtoInativo);

        _favoritoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId))
            .ReturnsAsync([favoritoAtivo, favoritoInativo]);

        var resultado = await _favoritoService.ListarDoUsuario(usuarioId);

        Assert.Single(resultado);
        Assert.Equal(produtoAtivo.ProdutoId, resultado[0].ProdutoId);
        Assert.True(resultado[0].EstaFavorito);
    }
}
