using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Helpers;
using DocesCabana.Application.Mappings;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class CupomServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 20);

    private readonly Mock<ICupomRepository> _cupomRepositoryMock = new();
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock = new();
    private readonly Mock<ISubcategoriaRepository> _subcategoriaRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<TimeProvider> _relogioMock = new();
    private readonly CupomService _service;

    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Categoria _doces = new("Doces");
    private readonly Categoria _bolos = new("Bolos");
    private readonly Subcategoria _brigadeiros;
    private readonly Subcategoria _bolosCaseiros;
    private readonly Produto _brigadeiro;
    private readonly Produto _beijinho;
    private readonly Produto _boloDeCenoura;

    public CupomServiceTests()
    {
        _brigadeiros = new Subcategoria(_doces.CategoriaId, "Brigadeiros");
        _bolosCaseiros = new Subcategoria(_bolos.CategoriaId, "Bolos caseiros");
        _brigadeiro = CriarProduto(_brigadeiros, "Brigadeiro", 10m);
        _beijinho = CriarProduto(_brigadeiros, "Beijinho", 9m);
        _boloDeCenoura = CriarProduto(_bolosCaseiros, "Bolo de cenoura", 40m);

        // 20/11/2026 às 12h em Brasília.
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.Zero));
        _subcategoriaRepositoryMock.Setup(r => r.BuscarTodos()).ReturnsAsync([_brigadeiros, _bolosCaseiros]);
        _pedidoRepositoryMock.Setup(r => r.ClienteJaUsouCupom(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        _service = new CupomService(
            _cupomRepositoryMock.Object, _pedidoRepositoryMock.Object,
            _subcategoriaRepositoryMock.Object, _relogioMock.Object, _unitOfWorkMock.Object);
    }

    private static Produto CriarProduto(Subcategoria subcategoria, string nome, decimal preco,
        ProdutoStatus status = ProdutoStatus.Ativo) =>
        new(subcategoria.SubcategoriaId, nome, preco, "https://exemplo.com/doce.png", 0.1m, 5m, 5m, 5m, status);

    private static CarrinhoDTO Carrinho(PromocoesVigentes? promocoes, params (Produto Produto, short Quantidade)[] pares) =>
        CarrinhoMapper.Montar(pares, null, promocoes);

    private static CarrinhoDTO Carrinho(params (Produto Produto, short Quantidade)[] pares) =>
        Carrinho(null, pares);

    private Cupom Cadastrar(
        string codigo = "BEMVINDO10",
        TipoDeDesconto tipo = TipoDeDesconto.Percentual,
        decimal valor = 10,
        short? leve = null,
        short? pague = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        decimal? valorMinimo = null,
        bool umUso = false,
        Categoria? categoria = null,
        Subcategoria? subcategoria = null)
    {
        var cupom = new Cupom(codigo, tipo, valor, leve, pague, inicio ?? Hoje, fim ?? Hoje.AddDays(30),
            valorMinimo, umUso, categoria?.CategoriaId, subcategoria?.SubcategoriaId);

        if (categoria is not null)
            typeof(Cupom).GetProperty(nameof(Cupom.Categoria))!.SetValue(cupom, categoria);

        if (subcategoria is not null)
            typeof(Cupom).GetProperty(nameof(Cupom.Subcategoria))!.SetValue(cupom, subcategoria);

        _cupomRepositoryMock.Setup(r => r.BuscarPorCodigo(cupom.Codigo)).ReturnsAsync(cupom);
        return cupom;
    }

    private void AssertRecusado(ResultadoDoCupomDTO resultado, string motivo)
    {
        Assert.False(resultado.Valido);
        Assert.Equal(motivo, resultado.Motivo);
        Assert.Equal(0m, resultado.DescontoTotal);
    }

    // Motivos de recusa (D11)

    [Fact]
    public async Task Dado_CodigoInexistente_Quando_Avaliar_Entao_MotivoDeveSerNaoEncontrado()
    {
        var resultado = await _service.Avaliar("naoexiste", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Cupom não encontrado.");
        Assert.Equal("NAOEXISTE", resultado.Codigo);
    }

    [Fact]
    public async Task Dado_CupomDesativado_Quando_Avaliar_Entao_MotivoDeveSerNaoEncontrado()
    {
        Cadastrar().Desativar();

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Cupom não encontrado.");
    }

    [Fact]
    public async Task Dado_CupomAgendado_Quando_Avaliar_Entao_MotivoDeveSerAindaNaoValendo()
    {
        Cadastrar(inicio: Hoje.AddDays(1), fim: Hoje.AddDays(5));

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Este cupom ainda não está valendo.");
    }

    [Fact]
    public async Task Dado_CupomEncerradoOntem_Quando_Avaliar_Entao_MotivoDeveSerExpirou()
    {
        Cadastrar(inicio: Hoje.AddDays(-5), fim: Hoje.AddDays(-1));

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Este cupom expirou.");
    }

    [Fact]
    public async Task Dado_CupomDeUsoUnicoJaUsado_Quando_Avaliar_Entao_MotivoDeveSerJaUsou()
    {
        var cupom = Cadastrar(umUso: true);
        _pedidoRepositoryMock.Setup(r => r.ClienteJaUsouCupom(_usuarioId, cupom.CupomId)).ReturnsAsync(true);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Você já usou este cupom.");
    }

    [Fact]
    public async Task Dado_CupomSemUsoUnicoJaUsado_Quando_Avaliar_Entao_DeveValer()
    {
        var cupom = Cadastrar(umUso: false);
        _pedidoRepositoryMock.Setup(r => r.ClienteJaUsouCupom(_usuarioId, cupom.CupomId)).ReturnsAsync(true);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        Assert.True(resultado.Valido);
    }

    [Fact]
    public async Task Dado_CupomDeCategoriaSemItensDela_Quando_Avaliar_Entao_MotivoDeveSerSoParaCategoria()
    {
        Cadastrar(categoria: _bolos);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        AssertRecusado(resultado, "Este cupom vale só para itens de Bolos.");
    }

    [Fact]
    public async Task Dado_BaseAbaixoDoMinimo_Quando_Avaliar_Entao_MotivoDeveTrazerOMinimo()
    {
        Cadastrar(valorMinimo: 80);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 3)), null);

        AssertRecusado(resultado, "Este cupom vale para compras a partir de R$ 80,00.");
    }

    [Fact]
    public async Task Dado_BaseDaCategoriaAbaixoDoMinimo_Quando_Avaliar_Entao_MotivoDeveTrazerMinimoECategoria()
    {
        Cadastrar(valorMinimo: 80, categoria: _doces);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 3), (_boloDeCenoura, 2)), null);

        AssertRecusado(resultado, "Este cupom vale para compras a partir de R$ 80,00 em Doces.");
    }

    [Fact]
    public async Task Dado_LeveXPagueYSemQuantidade_Quando_Avaliar_Entao_MotivoDevePedirMaisUnidades()
    {
        Cadastrar(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: 3, pague: 2);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 2), (_beijinho, 2)), null);

        AssertRecusado(resultado, "Leve 3 unidades de um mesmo produto para usar este cupom.");
    }

    // Cálculo (D4, D5, D6)

    [Fact]
    public async Task Dado_CupomPercentual_Quando_Avaliar_Entao_DeveDescontarDaBaseComRotulo()
    {
        var cupom = Cadastrar();

        var resultado = await _service.Avaliar(" bemvindo10 ", _usuarioId,
            Carrinho((_brigadeiro, 5), (_boloDeCenoura, 1)), 20m);

        Assert.True(resultado.Valido);
        Assert.Null(resultado.Motivo);
        Assert.Equal("BEMVINDO10", resultado.Codigo);
        Assert.Equal(cupom.CupomId, resultado.CupomId);
        Assert.Equal("10%", resultado.Rotulo);
        Assert.False(resultado.FreteGratis);
        Assert.Equal(9m, resultado.DescontoNosProdutos);
        Assert.Equal(0m, resultado.DescontoNoFrete);
    }

    [Fact]
    public async Task Dado_CupomValorFixo_Quando_Avaliar_Entao_DeveSerLimitadoPelaBase()
    {
        Cadastrar(tipo: TipoDeDesconto.ValorFixo, valor: 15);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        Assert.Equal(10m, resultado.DescontoNosProdutos);
        Assert.Equal("R$ 15,00", resultado.Rotulo);
    }

    [Fact]
    public async Task Dado_CupomLeveXPagueY_Quando_Avaliar_Entao_DeveDescontarPorLinha()
    {
        Cadastrar(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: 3, pague: 2);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 3), (_beijinho, 2)), null);

        Assert.True(resultado.Valido);
        Assert.Equal(10m, resultado.DescontoNosProdutos);
        Assert.Equal("Leve 3, pague 2", resultado.Rotulo);
    }

    [Fact]
    public async Task Dado_CupomFreteGratis_Quando_Avaliar_Entao_DeveDescontarOFreteInteiro()
    {
        Cadastrar(tipo: TipoDeDesconto.FreteGratis, valor: 0);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), 23.45m);

        Assert.True(resultado.Valido);
        Assert.True(resultado.FreteGratis);
        Assert.Equal("Frete grátis", resultado.Rotulo);
        Assert.Equal(0m, resultado.DescontoNosProdutos);
        Assert.Equal(23.45m, resultado.DescontoNoFrete);
        Assert.Equal(23.45m, resultado.DescontoTotal);
    }

    [Fact]
    public async Task Dado_CupomFreteGratisSemFreteCalculado_Quando_Avaliar_Entao_DeveValerSemDesconto()
    {
        Cadastrar(tipo: TipoDeDesconto.FreteGratis, valor: 0);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), null);

        Assert.True(resultado.Valido);
        Assert.True(resultado.FreteGratis);
        Assert.Equal(0m, resultado.DescontoTotal);
    }

    [Fact]
    public async Task Dado_CupomFreteGratisERetirada_Quando_Avaliar_Entao_DeveRecusar()
    {
        Cadastrar(tipo: TipoDeDesconto.FreteGratis, valor: 0);

        var resultado = await _service.Avaliar(
            "BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), 0m, ModalidadeDeEntrega.RetiradaNaLoja);

        Assert.False(resultado.Valido);
        Assert.Equal("Cupom de frete grátis não vale para retirada na loja.", resultado.Motivo);
    }

    [Fact]
    public async Task Dado_CupomPercentualERetirada_Quando_Avaliar_Entao_DeveValer()
    {
        Cadastrar(tipo: TipoDeDesconto.Percentual, valor: 10);

        var resultado = await _service.Avaliar(
            "BEMVINDO10", _usuarioId, Carrinho((_brigadeiro, 1)), 0m, ModalidadeDeEntrega.RetiradaNaLoja);

        Assert.True(resultado.Valido);
        Assert.Equal(1m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_LinhaComPromocaoECupomPercentual_Quando_Avaliar_Entao_BaseDeveSerOValorLiquido()
    {
        var promocao = new Promocao("Metade", TipoDeDesconto.Percentual, 50, null, null,
            Hoje, Hoje, null, null, [_brigadeiro]);
        var promocoes = new PromocoesVigentes([promocao],
            new Dictionary<Guid, Guid> { [_brigadeiros.SubcategoriaId] = _doces.CategoriaId });
        Cadastrar();

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho(promocoes, (_brigadeiro, 4)), null);

        Assert.Equal(2m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_CupomDeCategoriaComItensDeOutra_Quando_Avaliar_Entao_SoDeveDescontarOsDaCategoria()
    {
        Cadastrar(categoria: _doces);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 2), (_boloDeCenoura, 1)), null);

        Assert.Equal(2m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_CupomDeSubcategoriaComItensDeOutra_Quando_Avaliar_Entao_SoDeveDescontarOsDaSubcategoria()
    {
        Cadastrar(subcategoria: _bolosCaseiros);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 2), (_boloDeCenoura, 1)), null);

        Assert.Equal(4m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_CupomDeCategoriaComMinimoAtingidoSoPelaCategoria_Quando_Avaliar_Entao_DeveValer()
    {
        Cadastrar(valorMinimo: 80, categoria: _bolos);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 1), (_boloDeCenoura, 2)), null);

        Assert.True(resultado.Valido);
        Assert.Equal(8m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_LinhaIndisponivel_Quando_Avaliar_Entao_NaoDeveContarNaBase()
    {
        var esgotado = CriarProduto(_brigadeiros, "Esgotado", 100m, ProdutoStatus.ForaDeEstoque);
        Cadastrar();

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 1), (esgotado, 1)), null);

        Assert.Equal(1m, resultado.DescontoNosProdutos);
    }

    [Fact]
    public async Task Dado_LinhaIndisponivel_Quando_AvaliarMinimo_Entao_NaoDeveContar()
    {
        var esgotado = CriarProduto(_brigadeiros, "Esgotado", 100m, ProdutoStatus.ForaDeEstoque);
        Cadastrar(valorMinimo: 50);

        var resultado = await _service.Avaliar("BEMVINDO10", _usuarioId,
            Carrinho((_brigadeiro, 1), (esgotado, 1)), null);

        Assert.False(resultado.Valido);
    }

    // Gestão (D15, D16, D17)

    private static CupomDTO DtoValido() => new()
    {
        Codigo = " frete50 ",
        Tipo = TipoDeDesconto.FreteGratis,
        DataInicio = Hoje,
        DataFim = Hoje.AddDays(10),
        ValorMinimo = 50,
        UmUsoPorCliente = true,
    };

    [Fact]
    public async Task Dado_CodigoNovo_Quando_Cadastrar_Entao_DeveAdicionarNormalizadoESalvar()
    {
        Cupom? adicionado = null;
        _cupomRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Cupom>())).Callback<Cupom>(c => adicionado = c);

        await _service.Cadastrar(DtoValido());

        Assert.Equal("FRETE50", adicionado!.Codigo);
        Assert.Equal(TipoDeDesconto.FreteGratis, adicionado.Tipo);
        Assert.Equal(50m, adicionado.ValorMinimo);
        Assert.True(adicionado.UmUsoPorCliente);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_CodigoExistente_Quando_Cadastrar_Entao_DeveLancarSemSalvar()
    {
        _cupomRepositoryMock.Setup(r => r.ExisteCodigo("FRETE50")).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Cadastrar(DtoValido()));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CodigoMinusculo_Quando_CodigoJaExiste_Entao_DeveConsultarNormalizado()
    {
        _cupomRepositoryMock.Setup(r => r.ExisteCodigo("FRETE50")).ReturnsAsync(true);

        Assert.True(await _service.CodigoJaExiste(" frete50 "));
    }

    [Fact]
    public async Task Dado_CupomExistente_Quando_Atualizar_Entao_NaoDeveTrocarOCodigo()
    {
        var cupom = Cadastrar();
        _cupomRepositoryMock.Setup(r => r.BuscarPorId(cupom.CupomId)).ReturnsAsync(cupom);
        var dto = DtoValido();
        dto.CupomId = cupom.CupomId;
        dto.Codigo = "OUTRO";

        await _service.Atualizar(dto);

        Assert.Equal("BEMVINDO10", cupom.Codigo);
        Assert.Equal(TipoDeDesconto.FreteGratis, cupom.Tipo);
        Assert.Equal(50m, cupom.ValorMinimo);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_CupomInexistente_Quando_Atualizar_Entao_DeveLancarKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Atualizar(DtoValido()));
    }

    [Fact]
    public async Task Dado_CupomAtivo_Quando_Desativar_Entao_DeveDesativarESalvar()
    {
        var cupom = Cadastrar();
        _cupomRepositoryMock.Setup(r => r.BuscarPorId(cupom.CupomId)).ReturnsAsync(cupom);

        await _service.Desativar(cupom.CupomId);

        Assert.False(cupom.Ativo);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_CupomDesativado_Quando_Ativar_Entao_DeveAtivarESalvar()
    {
        var cupom = Cadastrar();
        cupom.Desativar();
        _cupomRepositoryMock.Setup(r => r.BuscarPorId(cupom.CupomId)).ReturnsAsync(cupom);

        await _service.Ativar(cupom.CupomId);

        Assert.True(cupom.Ativo);
    }

    [Fact]
    public async Task Dado_CuponsComUsos_Quando_ListarParaGestao_Entao_DeveTrazerUsosESituacao()
    {
        var vigente = Cadastrar(valorMinimo: 50, umUso: true, categoria: _doces);
        var agendado = Cadastrar(codigo: "NATAL", tipo: TipoDeDesconto.FreteGratis, valor: 0,
            inicio: Hoje.AddDays(5), fim: Hoje.AddDays(10));
        _cupomRepositoryMock.Setup(r => r.ListarParaGestao()).ReturnsAsync([vigente, agendado]);
        _pedidoRepositoryMock.Setup(r => r.ContarUsosPorCupom())
            .ReturnsAsync(new Dictionary<Guid, int> { [vigente.CupomId] = 3 });

        var lista = await _service.ListarParaGestao();

        var resumoVigente = lista.Single(c => c.Codigo == "BEMVINDO10");
        Assert.Equal(3, resumoVigente.Usos);
        Assert.Equal(SituacaoDaPromocao.Vigente, resumoVigente.Situacao);
        Assert.Equal("10%", resumoVigente.RotuloDoDesconto);
        Assert.Equal("A partir de R$ 50,00 · Categoria: Doces · 1 uso por cliente", resumoVigente.Restricoes);

        var resumoAgendado = lista.Single(c => c.Codigo == "NATAL");
        Assert.Equal(0, resumoAgendado.Usos);
        Assert.Equal(SituacaoDaPromocao.Agendada, resumoAgendado.Situacao);
        Assert.Equal("Sem restrições", resumoAgendado.Restricoes);
    }

    [Fact]
    public async Task Dado_CupomExistente_Quando_BuscarParaEdicao_Entao_DeveMapearOsCampos()
    {
        var cupom = Cadastrar(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: 3, pague: 2, subcategoria: _brigadeiros);
        _cupomRepositoryMock.Setup(r => r.BuscarPorId(cupom.CupomId)).ReturnsAsync(cupom);

        var dto = await _service.BuscarParaEdicao(cupom.CupomId);

        Assert.Equal("BEMVINDO10", dto.Codigo);
        Assert.Null(dto.Valor);
        Assert.Equal((short)3, dto.QuantidadeLeve);
        Assert.Equal(_brigadeiros.SubcategoriaId, dto.SubcategoriaId);
    }
}
