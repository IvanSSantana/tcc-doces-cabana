using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class PromocaoServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 20);

    private readonly Mock<IPromocaoRepository> _promocaoRepositoryMock = new();
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock = new();
    private readonly Mock<ISubcategoriaRepository> _subcategoriaRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<TimeProvider> _relogioMock = new();
    private readonly PromocaoService _service;

    private readonly Categoria _categoria = new("Doces");
    private readonly Subcategoria _subcategoria;
    private readonly Produto _produto;

    public PromocaoServiceTests()
    {
        _subcategoria = new Subcategoria(_categoria.CategoriaId, "Brigadeiros");
        _produto = new Produto(_subcategoria.SubcategoriaId, "Brigadeiro", 10m,
            "https://exemplo.com/brigadeiro.png", 0.1m, 5m, 5m, 5m);

        // 20/11/2026 às 12h em Brasília.
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.Zero));
        _subcategoriaRepositoryMock.Setup(r => r.BuscarTodos()).ReturnsAsync([_subcategoria]);

        _service = new PromocaoService(
            _promocaoRepositoryMock.Object, _produtoRepositoryMock.Object,
            _subcategoriaRepositoryMock.Object, _relogioMock.Object, _unitOfWorkMock.Object);
    }

    private Promocao PorCategoria(DateOnly inicio, DateOnly fim) =>
        new("Dez", TipoDeDesconto.Percentual, 10, null, null, inicio, fim, _categoria.CategoriaId, null, null);

    private PromocaoDTO DtoValido() => new()
    {
        Nome = "Semana do brigadeiro",
        Tipo = TipoDeDesconto.Percentual,
        Valor = 10,
        DataInicio = Hoje,
        DataFim = Hoje.AddDays(5),
        Abrangencia = AbrangenciaDaPromocao.ProdutosEscolhidos,
        ProdutoIds = [_produto.ProdutoId],
        CategoriaId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Dado_PromocaoVigentePorCategoria_Quando_CarregarVigentes_Entao_DeveAplicarAoProdutoDaCategoria()
    {
        _promocaoRepositoryMock.Setup(r => r.ListarVigentes(Hoje)).ReturnsAsync([PorCategoria(Hoje, Hoje)]);

        var vigentes = await _service.CarregarVigentes();

        var desconto = vigentes.MelhorPara(_produto.ProdutoId, _subcategoria.SubcategoriaId, 10m, 1);
        Assert.Equal(1m, desconto!.Valor);
    }

    [Fact]
    public async Task Dado_PromocaoEncerradaOntem_Quando_CarregarVigentes_Entao_NaoDeveAplicar()
    {
        _promocaoRepositoryMock.Setup(r => r.ListarVigentes(It.IsAny<DateOnly>()))
            .ReturnsAsync([PorCategoria(Hoje.AddDays(-3), Hoje.AddDays(-1))]);

        var vigentes = await _service.CarregarVigentes();

        Assert.Null(vigentes.MelhorPara(_produto.ProdutoId, _subcategoria.SubcategoriaId, 10m, 1));
    }

    [Fact]
    public async Task Dado_UtcAntesDas3h_Quando_CarregarVigentes_Entao_DeveConsultarComODiaDeBrasilia()
    {
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 21, 2, 0, 0, TimeSpan.Zero));
        _promocaoRepositoryMock.Setup(r => r.ListarVigentes(It.IsAny<DateOnly>())).ReturnsAsync([]);

        await _service.CarregarVigentes();

        _promocaoRepositoryMock.Verify(r => r.ListarVigentes(Hoje), Times.Once);
    }

    [Fact]
    public async Task Dado_NenhumaPromocaoVigente_Quando_CarregarVigentes_Entao_NaoDeveConsultarSubcategorias()
    {
        _promocaoRepositoryMock.Setup(r => r.ListarVigentes(Hoje)).ReturnsAsync([]);

        var vigentes = await _service.CarregarVigentes();

        Assert.Null(vigentes.MelhorPara(_produto.ProdutoId, _subcategoria.SubcategoriaId, 10m, 1));
        _subcategoriaRepositoryMock.Verify(r => r.BuscarTodos(), Times.Never);
    }

    [Fact]
    public async Task Dado_PromocaoValida_Quando_Cadastrar_Entao_DeveAdicionarESalvar()
    {
        _produtoRepositoryMock.Setup(r => r.BuscarPorIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([_produto]);
        Promocao? adicionada = null;
        _promocaoRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Promocao>()))
            .Callback<Promocao>(p => adicionada = p);

        await _service.Cadastrar(DtoValido());

        Assert.NotNull(adicionada);
        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, adicionada.Abrangencia);
        Assert.Null(adicionada.CategoriaId);
        Assert.Single(adicionada.Produtos);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_AbrangenciaCategoria_Quando_Cadastrar_Entao_DeveIgnorarProdutosDoFormulario()
    {
        var dto = DtoValido();
        dto.Abrangencia = AbrangenciaDaPromocao.Categoria;
        Promocao? adicionada = null;
        _promocaoRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Promocao>()))
            .Callback<Promocao>(p => adicionada = p);

        await _service.Cadastrar(dto);

        Assert.Equal(dto.CategoriaId, adicionada!.CategoriaId);
        Assert.Empty(adicionada.Produtos);
        _produtoRepositoryMock.Verify(r => r.BuscarPorIds(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Dado_LeveXPagueYSemValor_Quando_Cadastrar_Entao_DeveCriarComValorZero()
    {
        var dto = DtoValido();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.Valor = null;
        dto.QuantidadeLeve = 3;
        dto.QuantidadePague = 2;
        _produtoRepositoryMock.Setup(r => r.BuscarPorIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([_produto]);
        Promocao? adicionada = null;
        _promocaoRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Promocao>()))
            .Callback<Promocao>(p => adicionada = p);

        await _service.Cadastrar(dto);

        Assert.Equal(0, adicionada!.Valor);
        Assert.Equal((short)3, adicionada.QuantidadeLeve);
    }

    [Fact]
    public async Task Dado_ProdutoIdInexistente_Quando_Cadastrar_Entao_DeveLancarKeyNotFoundException()
    {
        var dto = DtoValido();
        dto.ProdutoIds = [_produto.ProdutoId, Guid.NewGuid()];
        _produtoRepositoryMock.Setup(r => r.BuscarPorIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([_produto]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Cadastrar(dto));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PromocaoExistente_Quando_Atualizar_Entao_DeveAlterarESalvar()
    {
        var promocao = PorCategoria(Hoje, Hoje);
        _promocaoRepositoryMock.Setup(r => r.BuscarComProdutos(promocao.PromocaoId)).ReturnsAsync(promocao);
        _produtoRepositoryMock.Setup(r => r.BuscarPorIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([_produto]);
        var dto = DtoValido();
        dto.PromocaoId = promocao.PromocaoId;

        await _service.Atualizar(dto);

        Assert.Equal("Semana do brigadeiro", promocao.Nome);
        Assert.Equal(Hoje.AddDays(5), promocao.DataFim);
        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, promocao.Abrangencia);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_PromocaoInexistente_Quando_Atualizar_Entao_DeveLancarKeyNotFoundException()
    {
        _promocaoRepositoryMock.Setup(r => r.BuscarComProdutos(It.IsAny<Guid>())).ReturnsAsync((Promocao?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Atualizar(DtoValido()));
    }

    [Fact]
    public async Task Dado_PromocaoAtiva_Quando_Desativar_Entao_DeveDesativarESalvar()
    {
        var promocao = PorCategoria(Hoje, Hoje);
        _promocaoRepositoryMock.Setup(r => r.BuscarPorId(promocao.PromocaoId)).ReturnsAsync(promocao);

        await _service.Desativar(promocao.PromocaoId);

        Assert.False(promocao.Ativa);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_PromocaoDesativada_Quando_Ativar_Entao_DeveAtivarESalvar()
    {
        var promocao = PorCategoria(Hoje, Hoje);
        promocao.Desativar();
        _promocaoRepositoryMock.Setup(r => r.BuscarPorId(promocao.PromocaoId)).ReturnsAsync(promocao);

        await _service.Ativar(promocao.PromocaoId);

        Assert.True(promocao.Ativa);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_PromocaoInexistente_Quando_Desativar_Entao_DeveLancarKeyNotFoundException()
    {
        _promocaoRepositoryMock.Setup(r => r.BuscarPorId(It.IsAny<Guid>())).ReturnsAsync((Promocao?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Desativar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Dado_PromocaoInexistente_Quando_BuscarParaEdicao_Entao_DeveLancarKeyNotFoundException()
    {
        _promocaoRepositoryMock.Setup(r => r.BuscarComProdutos(It.IsAny<Guid>())).ReturnsAsync((Promocao?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.BuscarParaEdicao(Guid.NewGuid()));
    }

    [Fact]
    public async Task Dado_PromocaoPorProdutos_Quando_BuscarParaEdicao_Entao_DeveDevolverOsIdsDosProdutos()
    {
        var promocao = new Promocao("Leve 3", TipoDeDesconto.CompreXLeveY, 0, 3, 2, Hoje, Hoje, null, null, [_produto]);
        _promocaoRepositoryMock.Setup(r => r.BuscarComProdutos(promocao.PromocaoId)).ReturnsAsync(promocao);

        var dto = await _service.BuscarParaEdicao(promocao.PromocaoId);

        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, dto.Abrangencia);
        Assert.Equal([_produto.ProdutoId], dto.ProdutoIds);
        Assert.Null(dto.Valor);
        Assert.Equal((short)3, dto.QuantidadeLeve);
    }

    [Fact]
    public async Task Dado_PromocoesEmVariasSituacoes_Quando_ListarParaGestao_Entao_DeveClassificarCadaUma()
    {
        var vigente = PorCategoria(Hoje, Hoje);
        var agendada = PorCategoria(Hoje.AddDays(1), Hoje.AddDays(2));
        var encerrada = PorCategoria(Hoje.AddDays(-2), Hoje.AddDays(-1));
        var desativada = PorCategoria(Hoje, Hoje);
        desativada.Desativar();
        _promocaoRepositoryMock.Setup(r => r.ListarParaGestao()).ReturnsAsync([vigente, agendada, encerrada, desativada]);

        var lista = await _service.ListarParaGestao();

        Assert.Equal(
            [SituacaoDaPromocao.Vigente, SituacaoDaPromocao.Agendada, SituacaoDaPromocao.Encerrada, SituacaoDaPromocao.Desativada],
            lista.Select(r => r.Situacao));
        Assert.Equal("-10%", lista[0].RotuloDoDesconto);
    }
}
