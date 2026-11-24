using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class BannerServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 12, 10);
    private const string Imagem = "https://exemplo.com/banner.png";

    private readonly Mock<IBannerRepository> _bannerRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<TimeProvider> _relogioMock = new();
    private readonly CacheFalso _cache = new();
    private readonly BannerService _service;

    private readonly Categoria _categoria = new("Empório");
    private readonly Subcategoria _subcategoria;
    private readonly Produto _produto;

    public BannerServiceTests()
    {
        _subcategoria = new Subcategoria(_categoria.CategoriaId, "Cafés Especiais");
        DefinirNavegacao(_subcategoria, nameof(Subcategoria.Categoria), _categoria);
        _produto = new Produto(_subcategoria.SubcategoriaId, "Bolo de Pote", 12m, "https://exemplo.com/bolo.png", 0.3m, 8m, 8m, 8m);

        // 10/12/2026 às 12h em Brasília.
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 12, 10, 15, 0, 0, TimeSpan.Zero));

        _service = new BannerService(_bannerRepositoryMock.Object, _relogioMock.Object, _unitOfWorkMock.Object, _cache);
    }

    private static void DefinirNavegacao(object entidade, string propriedade, object valor) =>
        entidade.GetType().GetProperty(propriedade)!.SetValue(entidade, valor);

    private Banner ParaCategoria(string texto = "Empório")
    {
        var banner = new Banner(Imagem, texto, 0, Hoje, null, _categoria.CategoriaId, null, null);
        DefinirNavegacao(banner, nameof(Banner.Categoria), _categoria);
        return banner;
    }

    private Banner ParaSubcategoria()
    {
        var banner = new Banner(Imagem, "Cafés", 1, Hoje, null, null, _subcategoria.SubcategoriaId, null);
        DefinirNavegacao(banner, nameof(Banner.Subcategoria), _subcategoria);
        return banner;
    }

    private Banner ParaProduto()
    {
        var banner = new Banner(Imagem, "Bolo", 2, Hoje, null, null, null, _produto.ProdutoId);
        DefinirNavegacao(banner, nameof(Banner.Produto), _produto);
        return banner;
    }

    private static BannerDTO DtoValido() => new()
    {
        TextoAlternativo = "Natal na Cabana",
        Ordem = 3,
        DataInicio = Hoje,
        DataFim = Hoje.AddDays(15),
        TipoDeDestino = TipoDeDestinoDoBanner.Produto,
        ProdutoId = Guid.NewGuid(),
        CategoriaId = Guid.NewGuid(),
        SubcategoriaId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Dado_BannersVigentes_Quando_ListarVigentesDoCarrossel_Entao_DeveMontarOsDestinos()
    {
        _bannerRepositoryMock.Setup(r => r.ListarVigentes(Hoje))
            .ReturnsAsync([ParaCategoria(), ParaSubcategoria(), ParaProduto()]);

        var banners = await _service.ListarVigentesDoCarrossel();

        Assert.Equal(3, banners.Count);
        Assert.Equal(new BannerDoCarrosselDTO(Imagem, "Empório", TipoDeDestinoDoBanner.Categoria, "emporio", null, null), banners[0]);
        Assert.Equal(new BannerDoCarrosselDTO(Imagem, "Cafés", TipoDeDestinoDoBanner.Subcategoria, "emporio", "cafes-especiais", null), banners[1]);
        Assert.Equal(new BannerDoCarrosselDTO(Imagem, "Bolo", TipoDeDestinoDoBanner.Produto, null, null, _produto.ProdutoId), banners[2]);
    }

    [Fact]
    public async Task Dado_UtcAntesDas3h_Quando_ListarVigentesDoCarrossel_Entao_DeveConsultarComODiaDeBrasilia()
    {
        // 11/12 às 01h UTC ainda é 10/12 em Brasília.
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 12, 11, 1, 0, 0, TimeSpan.Zero));
        _bannerRepositoryMock.Setup(r => r.ListarVigentes(It.IsAny<DateOnly>())).ReturnsAsync([]);

        await _service.ListarVigentesDoCarrossel();

        _bannerRepositoryMock.Verify(r => r.ListarVigentes(Hoje), Times.Once);
    }

    [Fact]
    public async Task Dado_VigentesJaLidos_Quando_ListarDeNovo_Entao_NaoDeveConsultarORepositorio()
    {
        _bannerRepositoryMock.Setup(r => r.ListarVigentes(Hoje)).ReturnsAsync([ParaCategoria()]);

        await _service.ListarVigentesDoCarrossel();
        await _service.ListarVigentesDoCarrossel();

        _bannerRepositoryMock.Verify(r => r.ListarVigentes(Hoje), Times.Once);
    }

    [Fact]
    public async Task Dado_BannerCadastrado_Quando_ListarVigentesDoCarrossel_Entao_DeveReconsultarRepositorio()
    {
        _bannerRepositoryMock.Setup(r => r.ListarVigentes(Hoje)).ReturnsAsync([]);

        await _service.ListarVigentesDoCarrossel();
        await _service.Cadastrar(DtoValido(), Imagem);
        await _service.ListarVigentesDoCarrossel();

        _bannerRepositoryMock.Verify(r => r.ListarVigentes(Hoje), Times.Exactly(2));
    }

    [Fact]
    public async Task Dado_DtoComTresDestinosPreenchidos_Quando_Cadastrar_Entao_DeveUsarSoODoTipoEscolhido()
    {
        var dto = DtoValido();
        Banner? adicionado = null;
        _bannerRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Banner>())).Callback<Banner>(b => adicionado = b);

        await _service.Cadastrar(dto, Imagem);

        Assert.NotNull(adicionado);
        Assert.Equal(dto.ProdutoId, adicionado.ProdutoId);
        Assert.Null(adicionado.CategoriaId);
        Assert.Null(adicionado.SubcategoriaId);
        Assert.Equal(Imagem, adicionado.ImagemUrl);
        Assert.Equal("Natal na Cabana", adicionado.TextoAlternativo);
        Assert.Equal(Hoje.AddDays(15), adicionado.DataFim);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_BannerExistenteSemImagemNova_Quando_Atualizar_Entao_DeveManterAImagemEAlterarOResto()
    {
        var banner = ParaCategoria();
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(banner.BannerId)).ReturnsAsync(banner);
        var dto = DtoValido();
        dto.BannerId = banner.BannerId;
        dto.TipoDeDestino = TipoDeDestinoDoBanner.Subcategoria;
        dto.DataFim = null;

        await _service.Atualizar(dto, null);

        Assert.Equal(Imagem, banner.ImagemUrl);
        Assert.Equal(dto.SubcategoriaId, banner.SubcategoriaId);
        Assert.Null(banner.CategoriaId);
        Assert.Equal((short)3, banner.Ordem);
        Assert.Null(banner.DataFim);
        Assert.Contains(ChavesDeCache.BannersVigentes, _cache.ChavesRemovidas);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_ImagemNova_Quando_Atualizar_Entao_DeveTrocarAImagem()
    {
        var banner = ParaCategoria();
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(banner.BannerId)).ReturnsAsync(banner);
        var dto = DtoValido();
        dto.BannerId = banner.BannerId;

        await _service.Atualizar(dto, "https://exemplo.com/nova.png");

        Assert.Equal("https://exemplo.com/nova.png", banner.ImagemUrl);
    }

    [Fact]
    public async Task Dado_BannerInexistente_Quando_Atualizar_Entao_DeveLancarKeyNotFoundException()
    {
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(It.IsAny<Guid>())).ReturnsAsync((Banner?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Atualizar(DtoValido(), null));
    }

    [Fact]
    public async Task Dado_BannerAtivo_Quando_Desativar_Entao_DeveDesativarSalvarELimparOCache()
    {
        var banner = ParaCategoria();
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(banner.BannerId)).ReturnsAsync(banner);

        await _service.Desativar(banner.BannerId);

        Assert.False(banner.Ativo);
        Assert.Contains(ChavesDeCache.BannersVigentes, _cache.ChavesRemovidas);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_BannerDesativado_Quando_Ativar_Entao_DeveAtivarESalvar()
    {
        var banner = ParaCategoria();
        banner.Desativar();
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(banner.BannerId)).ReturnsAsync(banner);

        await _service.Ativar(banner.BannerId);

        Assert.True(banner.Ativo);
        Assert.Contains(ChavesDeCache.BannersVigentes, _cache.ChavesRemovidas);
    }

    [Fact]
    public async Task Dado_BannerInexistente_Quando_Desativar_Entao_DeveLancarKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Desativar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Dado_BannerInexistente_Quando_BuscarParaEdicao_Entao_DeveLancarKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.BuscarParaEdicao(Guid.NewGuid()));
    }

    [Fact]
    public async Task Dado_BannerDeSubcategoria_Quando_BuscarParaEdicao_Entao_DeveDevolverOsDados()
    {
        var banner = ParaSubcategoria();
        _bannerRepositoryMock.Setup(r => r.BuscarPorId(banner.BannerId)).ReturnsAsync(banner);

        var dto = await _service.BuscarParaEdicao(banner.BannerId);

        Assert.Equal(banner.BannerId, dto.BannerId);
        Assert.Equal(TipoDeDestinoDoBanner.Subcategoria, dto.TipoDeDestino);
        Assert.Equal(_subcategoria.SubcategoriaId, dto.SubcategoriaId);
        Assert.Equal("Cafés", dto.TextoAlternativo);
        Assert.Equal((short)1, dto.Ordem);
        Assert.Equal(Imagem, dto.ImagemUrl);
    }

    [Fact]
    public async Task Dado_BannersEmVariasSituacoes_Quando_ListarParaGestao_Entao_DeveClassificarEDescreverODestino()
    {
        var vigente = ParaCategoria("Vigente");
        var agendado = ParaSubcategoria();
        agendado.AlterarPeriodo(Hoje.AddDays(1), null);
        var encerrado = ParaProduto();
        encerrado.AlterarPeriodo(Hoje.AddDays(-5), Hoje.AddDays(-1));
        var desativado = ParaCategoria("Desativado");
        desativado.Desativar();
        _bannerRepositoryMock.Setup(r => r.ListarParaGestao()).ReturnsAsync([vigente, agendado, encerrado, desativado]);

        var lista = await _service.ListarParaGestao();

        Assert.Equal(
            [SituacaoDoBanner.Vigente, SituacaoDoBanner.Agendado, SituacaoDoBanner.Encerrado, SituacaoDoBanner.Desativado],
            lista.Select(r => r.Situacao));
        Assert.Equal("Categoria: Empório", lista[0].Destino);
        Assert.Equal("Subcategoria: Empório › Cafés Especiais", lista[1].Destino);
        Assert.Equal("Produto: Bolo de Pote", lista[2].Destino);
    }
}
