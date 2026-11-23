using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class PainelDeVendasServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock = new();
    private readonly Mock<TimeProvider> _relogioMock = new();
    private readonly PainelDeVendasService _servico;

    // 23/11/2026, 12h em Brasília.
    private static readonly DateTimeOffset AgoraUtc = new(2026, 11, 23, 15, 0, 0, TimeSpan.Zero);

    public PainelDeVendasServiceTests()
    {
        _relogioMock.Setup(r => r.GetUtcNow()).Returns(AgoraUtc);
        _pedidoRepositoryMock
            .Setup(r => r.ListarReceitaEntre(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([]);
        _servico = new PainelDeVendasService(_pedidoRepositoryMock.Object, _relogioMock.Object);
    }

    private static DateTime Utc(int ano, int mes, int dia, int hora = 0) =>
        new(ano, mes, dia, hora, 0, 0, DateTimeKind.Utc);

    /// <summary>Pedido com data em UTC e itens (produto, nome, quantidade, preço).</summary>
    private static Pedido CriarPedido(
        DateTime dataUtc, decimal valorDoFrete = 10m, params (Guid ProdutoId, string Nome, short Quantidade, decimal Preco)[] itens)
    {
        var pedido = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 0m, valorDoFrete, "Correios", "PAC", 3, 7);
        foreach (var (produtoId, nome, quantidade, preco) in itens)
        {
            pedido.AcrescentarItem(produtoId, quantidade, preco);
            var item = pedido.Itens.Last();
            var produto = new Produto(Guid.NewGuid(), nome, preco, "https://imagem.com/p.jpg", 0.5m, 10m, 15m, 20m, id: produtoId);
            typeof(ItemPedido).GetProperty(nameof(ItemPedido.Produto))!.SetValue(item, produto);
        }

        typeof(Pedido).GetProperty(nameof(Pedido.Data))!.SetValue(pedido, dataUtc);
        return pedido;
    }

    private static Pedido PedidoSimples(DateTime dataUtc, decimal valorDosProdutos) =>
        CriarPedido(dataUtc, 10m, (Guid.NewGuid(), "Brigadeiro", 1, valorDosProdutos));

    private void PrepararPedidos(DateTime inicioUtc, DateTime fimUtc, params Pedido[] pedidos) =>
        _pedidoRepositoryMock.Setup(r => r.ListarReceitaEntre(inicioUtc, fimUtc)).ReturnsAsync(pedidos.ToList());

    [Fact]
    public async Task Dado_ReferenciaNula_Quando_Montar_Entao_DeveUsarHojeDeBrasiliaEBuscarOIntervaloEmUtc()
    {
        var painel = await _servico.Montar(PeriodoDoPainel.Mes, null);

        Assert.Equal(new DateOnly(2026, 11, 1), painel.Referencia);
        Assert.Equal("novembro de 2026", painel.RotuloDoPeriodo);
        _pedidoRepositoryMock.Verify(r => r.ListarReceitaEntre(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3)), Times.Once);
        _pedidoRepositoryMock.Verify(r => r.ListarReceitaEntre(Utc(2026, 10, 1, 3), Utc(2026, 11, 1, 3)), Times.Once);
    }

    [Fact]
    public async Task Dado_PedidoComFreteECupom_Quando_Montar_Entao_ReceitaDeveSerValorDosProdutos()
    {
        var produtoId = Guid.NewGuid();
        var pedido = CriarPedido(Utc(2026, 11, 10, 15), 12m, (produtoId, "Brigadeiro", 2, 10m));
        pedido.AplicarCupom(Guid.NewGuid(), "BEMVINDO10", 2m, 0m);
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), pedido);

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(18m, painel.Receita.Atual);
        Assert.Equal(1, painel.Pedidos.Atual);
        Assert.Equal(18m, painel.TicketMedio.Atual);
    }

    [Fact]
    public async Task Dado_PeriodoSemPedidos_Quando_Montar_Entao_TicketMedioDeveSerZero()
    {
        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(0m, painel.Receita.Atual);
        Assert.Equal(0m, painel.Pedidos.Atual);
        Assert.Equal(0m, painel.TicketMedio.Atual);
    }

    [Fact]
    public async Task Dado_PeriodoAnteriorComVendas_Quando_Montar_Entao_DeveCompararEDarVariacaoPercentual()
    {
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), PedidoSimples(Utc(2026, 11, 5, 15), 150m));
        PrepararPedidos(Utc(2026, 10, 1, 3), Utc(2026, 11, 1, 3),
            PedidoSimples(Utc(2026, 10, 5, 15), 50m), PedidoSimples(Utc(2026, 10, 6, 15), 50m));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(100m, painel.Receita.Anterior);
        Assert.Equal(50m, painel.Receita.VariacaoPercentual);
        Assert.Equal(-50m, painel.Pedidos.VariacaoPercentual);
        Assert.Equal(200m, painel.TicketMedio.VariacaoPercentual);
    }

    [Fact]
    public async Task Dado_PeriodoAnteriorSemVendas_Quando_Montar_Entao_VariacaoDeveSerNula()
    {
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), PedidoSimples(Utc(2026, 11, 5, 15), 150m));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Null(painel.Receita.VariacaoPercentual);
        Assert.Null(painel.Pedidos.VariacaoPercentual);
    }

    [Fact]
    public async Task Dado_MesSemVendasEmAlgunsDias_Quando_Montar_Entao_SerieDeveTerTodosOsDiasDoMes()
    {
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), PedidoSimples(Utc(2026, 11, 5, 15), 30m));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(30, painel.Serie.Count);
        Assert.Equal("05", painel.Serie[4].Rotulo);
        Assert.Equal(30m, painel.Serie[4].Receita);
        Assert.Equal(1, painel.Serie[4].Pedidos);
        Assert.Equal(0m, painel.Serie[5].Receita);
    }

    [Fact]
    public async Task Dado_PedidoAs23hDeBrasilia_Quando_Montar_Entao_DeveCairNoMesmoDia()
    {
        // 06/11 às 2h UTC = 05/11 às 23h em Brasília.
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), PedidoSimples(Utc(2026, 11, 6, 2), 30m));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(30m, painel.Serie[4].Receita);
        Assert.Equal(0m, painel.Serie[5].Receita);
        Assert.Equal(30m, painel.PorHora[23].Receita);
    }

    [Theory]
    [InlineData(PeriodoDoPainel.Dia, 24, "00h")]
    [InlineData(PeriodoDoPainel.Semana, 7, "seg 23/11")]
    [InlineData(PeriodoDoPainel.Ano, 12, "jan")]
    public async Task Dado_Periodo_Quando_Montar_Entao_SerieDeveTerAsFaixasDoPeriodo(
        PeriodoDoPainel periodo, int faixas, string primeiroRotulo)
    {
        var painel = await _servico.Montar(periodo, new DateOnly(2026, 11, 23));

        Assert.Equal(faixas, painel.Serie.Count);
        Assert.Equal(primeiroRotulo, painel.Serie[0].Rotulo);
    }

    [Fact]
    public async Task Dado_ProdutosEmpatadosNaQuantidade_Quando_Montar_Entao_DeveDesempatarPorReceita()
    {
        var barato = Guid.NewGuid();
        var caro = Guid.NewGuid();
        var maisVendido = Guid.NewGuid();
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3),
            CriarPedido(Utc(2026, 11, 5, 15), 10m, (barato, "Bala", 2, 1m), (caro, "Bolo", 2, 50m)),
            CriarPedido(Utc(2026, 11, 6, 15), 10m, (maisVendido, "Brigadeiro", 3, 2m)),
            CriarPedido(Utc(2026, 11, 7, 15), 10m, (maisVendido, "Brigadeiro", 1, 2m)));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(new[] { "Brigadeiro", "Bolo", "Bala" }, painel.MaisVendidos.Select(p => p.Nome));
        Assert.Equal(4, painel.MaisVendidos[0].Quantidade);
        Assert.Equal(8m, painel.MaisVendidos[0].Receita);
    }

    [Fact]
    public async Task Dado_MaisDeDezProdutos_Quando_Montar_Entao_DeveListarSoOsDez()
    {
        var itens = Enumerable.Range(1, 12)
            .Select(i => (Guid.NewGuid(), $"Produto {i:D2}", (short)i, 1m))
            .ToArray();
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3), CriarPedido(Utc(2026, 11, 5, 15), 10m, itens));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(10, painel.MaisVendidos.Count);
        Assert.Equal("Produto 12", painel.MaisVendidos[0].Nome);
    }

    [Fact]
    public async Task Dado_PedidosEmDiasEHorasDiferentes_Quando_Montar_Entao_PicosDevemSomarPorDiaDaSemanaEHora()
    {
        // 23/11/2026 é segunda; 14h UTC = 11h em Brasília.
        PrepararPedidos(Utc(2026, 11, 1, 3), Utc(2026, 12, 1, 3),
            PedidoSimples(Utc(2026, 11, 23, 14), 10m),
            PedidoSimples(Utc(2026, 11, 16, 14), 20m),
            PedidoSimples(Utc(2026, 11, 22, 20), 5m));

        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 1));

        Assert.Equal(7, painel.PorDiaDaSemana.Count);
        Assert.Equal("seg", painel.PorDiaDaSemana[0].Rotulo);
        Assert.Equal(30m, painel.PorDiaDaSemana[0].Receita);
        Assert.Equal(2, painel.PorDiaDaSemana[0].Pedidos);
        Assert.Equal("dom", painel.PorDiaDaSemana[6].Rotulo);
        Assert.Equal(5m, painel.PorDiaDaSemana[6].Receita);
        Assert.Equal(24, painel.PorHora.Count);
        Assert.Equal(30m, painel.PorHora[11].Receita);
        Assert.Equal(5m, painel.PorHora[17].Receita);
    }

    [Fact]
    public async Task Dado_PeriodoDia_Quando_Montar_Entao_PicosDevemFicarVazios()
    {
        var painel = await _servico.Montar(PeriodoDoPainel.Dia, new DateOnly(2026, 11, 23));

        Assert.Empty(painel.PorDiaDaSemana);
        Assert.Empty(painel.PorHora);
    }

    [Fact]
    public async Task Dado_PeriodoCorrente_Quando_Montar_Entao_NaoDeveHaverReferenciaSeguinte()
    {
        var painel = await _servico.Montar(PeriodoDoPainel.Mes, new DateOnly(2026, 11, 23));

        Assert.Equal(new DateOnly(2026, 10, 1), painel.ReferenciaAnterior);
        Assert.Null(painel.ReferenciaSeguinte);
    }

    [Fact]
    public async Task Dado_PeriodoPassado_Quando_Montar_Entao_ReferenciaSeguinteDeveSerOInicioDoProximo()
    {
        var painel = await _servico.Montar(PeriodoDoPainel.Semana, new DateOnly(2026, 11, 12));

        Assert.Equal(new DateOnly(2026, 11, 9), painel.Referencia);
        Assert.Equal(new DateOnly(2026, 11, 2), painel.ReferenciaAnterior);
        Assert.Equal(new DateOnly(2026, 11, 16), painel.ReferenciaSeguinte);
    }

    [Fact]
    public async Task Dado_PeriodoForaDoEnum_Quando_Montar_Entao_DeveCairNoMes()
    {
        var painel = await _servico.Montar((PeriodoDoPainel)99, new DateOnly(2026, 11, 23));

        Assert.Equal(PeriodoDoPainel.Mes, painel.Periodo);
        Assert.Equal(new DateOnly(2026, 11, 1), painel.Referencia);
    }
}
