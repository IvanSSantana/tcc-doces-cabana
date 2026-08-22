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
    private readonly Mock<IFavoritoRepository> _favoritoRepositoryMock;
    private readonly CatalogoService _catalogoService;

    private readonly Guid _categoriaDocesId = Guid.NewGuid();
    private readonly Guid _subcategoriaBarrasId = Guid.NewGuid();
    private readonly Guid _subcategoriaPotesId = Guid.NewGuid();

    public CatalogoServiceTests()
    {
        _categoriaServiceMock = new Mock<ICategoriaService>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _favoritoRepositoryMock = new Mock<IFavoritoRepository>();
        _catalogoService = new CatalogoService(_categoriaServiceMock.Object, _produtoRepositoryMock.Object, _favoritoRepositoryMock.Object);

        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>
            {
                new()
                {
                    CategoriaId = _categoriaDocesId,
                    Nome = "Doces",
                    Apelido = "doces",
                    Subcategorias =
                    [
                        new() { SubcategoriaId = _subcategoriaBarrasId, Nome = "Barras", Apelido = "barras" },
                        new() { SubcategoriaId = _subcategoriaPotesId, Nome = "Potes", Apelido = "potes" },
                        // Mesmo apelido de uma subcategoria de Empório
                        // (spec 016, RN-03) — a colisão só existiria se a
                        // resolução vazasse entre categorias.
                        new() { SubcategoriaId = Guid.NewGuid(), Nome = "Cappuccino", Apelido = "cappuccino" },
                    ]
                },
                new()
                {
                    CategoriaId = Guid.NewGuid(),
                    Nome = "Adega",
                    Apelido = "adega",
                    Subcategorias =
                    [
                        new() { SubcategoriaId = Guid.NewGuid(), Nome = "Cappuccino", Apelido = "cappuccino" },
                    ]
                },
            });
    }

    private static CriteriosDoCatalogoDTO CriteriosPadrao(
        string? apelidoDaCategoria = null,
        IReadOnlyCollection<string>? apelidosDeSubcategoria = null,
        bool apenasSemAcucar = false,
        string? termo = null) =>
        new(apelidoDaCategoria, apelidosDeSubcategoria ?? [], apenasSemAcucar, OrdenacaoCatalogo.NomeAZ, termo);

    [Fact]
    public async Task Dado_ApelidoInexistente_Quando_Montar_Entao_DeveLancarKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _catalogoService.Montar(CriteriosPadrao("inexistente"), 1));
    }

    [Fact]
    public async Task Dado_ApelidoValido_Quando_Montar_Entao_DeveFiltrarPelaCategoria()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 1);

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

        var resultado = await _catalogoService.Montar(CriteriosPadrao(), 1);

        Assert.Null(resultado.CategoriaAtual);
        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.CategoriaId == null)), Times.Once);
    }

    [Fact]
    public async Task Dado_DoisApelidosDeSubcategoriaMarcados_Quando_Montar_Entao_DevePassarOsDoisIdentificadoresAoRepositorio()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao("doces", ["barras", "potes"]);
        await _catalogoService.Montar(criterios, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.SubcategoriaIds.Count == 2
                && f.SubcategoriaIds.Contains(_subcategoriaBarrasId)
                && f.SubcategoriaIds.Contains(_subcategoriaPotesId))), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidoDeSubcategoriaDesconhecidoNaCategoria_Quando_Montar_Entao_DeveIgnorarEMostrarACategoriaInteira()
    {
        // RN-04 (spec 016): filtro que não pode ser aplicado não impede a
        // página — só a categoria (o endereço em si) produz "não encontrado".
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao("doces", ["subcategoria-que-nao-existe"]);
        var resultado = await _catalogoService.Montar(criterios, 1);

        Assert.NotNull(resultado.CategoriaAtual);
        Assert.Empty(resultado.SubcategoriasMarcadas);
        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.SubcategoriaIds.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Dado_MesmoApelidoDeSubcategoriaEmDuasCategorias_Quando_Montar_Entao_NaoDeveConfundir()
    {
        // RN-03: "cappuccino" existe em Doces e em Adega. Resolver dentro de
        // Doces não pode acidentalmente casar com a subcategoria de Adega.
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var categorias = await _categoriaServiceMock.Object.ListarComSubcategorias();
        var idCappuccinoDeDoces = categorias.Single(c => c.Apelido == "doces").Subcategorias.Single(s => s.Apelido == "cappuccino").SubcategoriaId;
        var idCappuccinoDeAdega = categorias.Single(c => c.Apelido == "adega").Subcategorias.Single(s => s.Apelido == "cappuccino").SubcategoriaId;

        var criterios = CriteriosPadrao("doces", ["cappuccino"]);
        await _catalogoService.Montar(criterios, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.SubcategoriaIds.Count == 1
                && f.SubcategoriaIds.Contains(idCappuccinoDeDoces)
                && !f.SubcategoriaIds.Contains(idCappuccinoDeAdega))), Times.Once);
    }

    [Fact]
    public async Task Dado_ApelidosDeSubcategoriaSemCategoria_Quando_Montar_Entao_DeveIgnorarPorNaoTerContraOQueComparar()
    {
        // Fora de uma categoria, RN-03 não tem escopo contra o que resolver
        // um apelido — o catálogo completo não filtra por subcategoria.
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao(apelidoDaCategoria: null, apelidosDeSubcategoria: ["barras"]);
        var resultado = await _catalogoService.Montar(criterios, 1);

        Assert.Empty(resultado.SubcategoriasMarcadas);
        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.SubcategoriaIds.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Dado_ApenasSemAcucarMarcado_Quando_Montar_Entao_DevePassarAoRepositorio()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao("doces", [], apenasSemAcucar: true);
        await _catalogoService.Montar(criterios, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.ApenasSemAcucar)), Times.Once);
    }

    [Fact]
    public async Task Dado_TermoComAcentoECaixaAlta_Quando_Montar_Entao_DevePassarNormalizadoAoRepositorio()
    {
        // RN-02 (spec 016): a comparação ignora acento e caixa dos dois
        // lados — é CatalogoService quem normaliza o termo antes de
        // repassar, o repositório só compara texto já normalizado.
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao(termo: "CAFÉ");
        await _catalogoService.Montar(criterios, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.TermoNormalizado == "cafe")), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Dado_TermoNuloOuEmBranco_Quando_Montar_Entao_NaoDeveVirarFiltro(string? termo)
    {
        // RF-09: buscar com o campo vazio é o catálogo completo, sem erro.
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao(termo: termo);
        await _catalogoService.Montar(criterios, 1);

        _produtoRepositoryMock.Verify(r => r.ContarNoCatalogo(
            It.Is<FiltroCatalogoDTO>(f => f.TermoNormalizado == null)), Times.Once);
    }

    [Fact]
    public async Task Dado_UmaBuscaComTermo_Quando_Montar_Entao_CatalogoDTODeveDevolverOTermoCru()
    {
        // RF-06: a tela reexibe o que a pessoa digitou, não o normalizado.
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var criterios = CriteriosPadrao(termo: "Brigadeiro");
        var resultado = await _catalogoService.Montar(criterios, 1);

        Assert.Equal("Brigadeiro", resultado.Termo);
    }

    [Fact]
    public async Task Dado_ProdutoInexistenteNaCategoria_Quando_Montar_Entao_PaginaDeveVirVazia()
    {
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(0);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([]);

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 1);

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

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 99);

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

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 0);

        Assert.Equal(1, resultado.Pagina.PaginaAtual);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Montar_Entao_DeveMarcarOsProdutosFavoritados()
    {
        // RF-02 (spec 015): o cartão precisa saber se o produto já está
        // favoritado por quem está vendo.
        var produtoFavoritado = CriarProduto("Favoritado");
        var produtoNaoFavoritado = CriarProduto("Não favoritado");
        var usuarioId = Guid.NewGuid();

        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(2);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([produtoFavoritado, produtoNaoFavoritado]);
        _favoritoRepositoryMock.Setup(r => r.IdsPorUsuario(usuarioId, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new HashSet<Guid> { produtoFavoritado.ProdutoId });

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 1, usuarioId);

        var dtoFavoritado = resultado.Pagina.Itens.Single(p => p.ProdutoId == produtoFavoritado.ProdutoId);
        var dtoNaoFavoritado = resultado.Pagina.Itens.Single(p => p.ProdutoId == produtoNaoFavoritado.ProdutoId);
        Assert.True(dtoFavoritado.EstaFavorito);
        Assert.False(dtoNaoFavoritado.EstaFavorito);

        // Uma consulta para a página inteira, não uma por produto (plano §5).
        _favoritoRepositoryMock.Verify(r => r.IdsPorUsuario(usuarioId, It.IsAny<IEnumerable<Guid>>()), Times.Once);
    }

    [Fact]
    public async Task Dado_Visitante_Quando_Montar_Entao_NenhumProdutoDeveVirMarcadoENaoDeveConsultarFavoritos()
    {
        var produto = CriarProduto("Produto");
        _produtoRepositoryMock.Setup(r => r.ContarNoCatalogo(It.IsAny<FiltroCatalogoDTO>())).ReturnsAsync(1);
        _produtoRepositoryMock.Setup(r => r.BuscarPaginaDoCatalogo(It.IsAny<FiltroCatalogoDTO>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync([produto]);

        var resultado = await _catalogoService.Montar(CriteriosPadrao("doces"), 1, usuarioId: null);

        Assert.False(resultado.Pagina.Itens.Single().EstaFavorito);
        _favoritoRepositoryMock.Verify(r => r.IdsPorUsuario(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    private static Produto CriarProduto(string nome) =>
        new(Guid.NewGuid(), nome, 10m, "https://imagem.com/produto.jpg");
}
