using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class CatalogoServiceTests
{
    private readonly Mock<ICategoriaService> _categoriaServiceMock;
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly CatalogoService _catalogoService;

    private readonly Guid _categoriaDocesId = Guid.NewGuid();

    public CatalogoServiceTests()
    {
        _categoriaServiceMock = new Mock<ICategoriaService>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _catalogoService = new CatalogoService(_categoriaServiceMock.Object, _produtoRepositoryMock.Object);

        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>
            {
                new() { CategoriaId = _categoriaDocesId, Nome = "Doces", Apelido = "doces" },
                new() { CategoriaId = Guid.NewGuid(), Nome = "Adega", Apelido = "adega" },
            });
    }

    [Fact]
    public async Task Dado_ApelidoInexistente_Quando_Montar_Entao_DeveLancarKeyNotFoundException()
    {
        var filtro = FiltroPadrao();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _catalogoService.Montar("inexistente", filtro, 1));
    }

    [Fact]
    public async Task Dado_ApelidoValido_Quando_Montar_Entao_DeveFiltrarPelaCategoria()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar("doces", FiltroPadrao(), 1);

        Assert.NotNull(resultado.CategoriaAtual);
        Assert.Equal(_categoriaDocesId, resultado.CategoriaAtual!.CategoriaId);
        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.CategoriaId == _categoriaDocesId)), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidoNulo_Quando_Montar_Entao_DeveSerOCatalogoCompleto()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar(null, FiltroPadrao(), 1);

        Assert.Null(resultado.CategoriaAtual);
        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.CategoriaId == null)), Times.Once);
    }

    [Fact]
    public async Task Dado_DuasSubcategoriasMarcadas_Quando_Montar_Entao_DevePassarAsDuasAoRepositorio()
    {
        var idBarras = Guid.NewGuid();
        var idPotes = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var filtro = new FiltroCatalogoDTO(null, [idBarras, idPotes], false, OrdenacaoCatalogo.NomeAZ);
        await _catalogoService.Montar("doces", filtro, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.SubcategoriaIds.Count == 2
                && f.SubcategoriaIds.Contains(idBarras)
                && f.SubcategoriaIds.Contains(idPotes))), Times.Once);
    }

    [Fact]
    public async Task Dado_ApenasSemAcucarMarcado_Quando_Montar_Entao_DevePassarAoRepositorio()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var filtro = new FiltroCatalogoDTO(null, [], true, OrdenacaoCatalogo.NomeAZ);
        await _catalogoService.Montar("doces", filtro, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.ApenasSemAcucar)), Times.Once);
    }

    [Fact]
    public async Task Dado_ProdutoInexistenteNaCategoria_Quando_Montar_Entao_PaginaDeveVirVazia()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar("doces", FiltroPadrao(), 1);

        Assert.Empty(resultado.Pagina.Itens);
        Assert.Equal(0, resultado.Pagina.TotalDeItens);
        Assert.Equal(1, resultado.Pagina.TotalDePaginas);
    }

    [Fact]
    public async Task Dado_PaginaAlemDoTotal_Quando_Montar_Entao_DeveLimitarAUltimaValida()
    {
        // 25 produtos, 12 por página -> 3 páginas. Pedir a página 99 deve
        // resultar na 3, não numa consulta vazia (RF-21).
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(25);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), 3, CatalogoService.TamanhoDaPagina))
            .ReturnsAsync([CriarProduto("Produto da página 3")]);

        var resultado = await _catalogoService.Montar("doces", FiltroPadrao(), 99);

        Assert.Equal(3, resultado.Pagina.PaginaAtual);
        Assert.Equal(3, resultado.Pagina.TotalDePaginas);
        _produtoRepositoryMock.Verify(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), 3, CatalogoService.TamanhoDaPagina), Times.Once);
    }

    [Fact]
    public async Task Dado_PaginaMenorQueUm_Quando_Montar_Entao_DeveLimitarAPrimeira()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(5);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), 1, CatalogoService.TamanhoDaPagina))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar("doces", FiltroPadrao(), 0);

        Assert.Equal(1, resultado.Pagina.PaginaAtual);
    }

    private static FiltroCatalogoDTO FiltroPadrao() =>
        new(null, [], false, OrdenacaoCatalogo.NomeAZ);

    private static Produto CriarProduto(string nome) =>
        new(Guid.NewGuid(), nome, 10m, "https://imagem.com/produto.jpg");
}
