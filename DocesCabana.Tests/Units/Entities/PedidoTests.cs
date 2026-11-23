using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Tests.Units.Entities;

public class PedidoTests
{
    private readonly Guid _usuarioValido = Guid.NewGuid();
    private readonly Guid _enderecoValido = Guid.NewGuid();
    private const decimal _valorDoFreteValido = 10.00m;
    private const string _transportadoraValida = "Correios";
    private const string _servicoValido = "PAC";
    private const int _prazoMinimoValido = 3;
    private const int _prazoMaximoValido = 7;

    private Pedido CriarPedido(
        decimal valor = 50.00m, decimal valorDoFrete = _valorDoFreteValido,
        string transportadora = _transportadoraValida, string servico = _servicoValido,
        int prazoMinimoEmDias = _prazoMinimoValido, int prazoMaximoEmDias = _prazoMaximoValido) =>
        new(_usuarioValido, _enderecoValido, valor, valorDoFrete, transportadora, servico, prazoMinimoEmDias, prazoMaximoEmDias);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPedido_Entao_DeveNascerConfirmadoComPagamentoNaoAprovado()
    {
        var pedido = CriarPedido(valor: 50.00m);

        Assert.Equal(PedidoStatus.Confirmado, pedido.Status);
        Assert.False(pedido.PagamentoAprovado);
        Assert.Equal(50.00m, pedido.Valor);
        Assert.True((DateTime.UtcNow - pedido.Data) < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pedido(
            Guid.Empty, _enderecoValido, 50.00m, _valorDoFreteValido, _transportadoraValida, _servicoValido, _prazoMinimoValido, _prazoMaximoValido));
    }

    [Fact]
    public void Dado_EnderecoInvalido_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pedido(
            _usuarioValido, Guid.Empty, 50.00m, _valorDoFreteValido, _transportadoraValida, _servicoValido, _prazoMinimoValido, _prazoMaximoValido));
    }

    [Fact]
    public void Dado_ValorNegativo_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(valor: -1m));
    }

    [Fact]
    public void Dado_ValorZero_Quando_CriarPedido_Entao_DeveConstruir()
    {
        var pedido = CriarPedido(valor: 0m);

        Assert.Equal(0m, pedido.Valor);
    }

    [Fact]
    public void Dado_FreteNegativo_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(valorDoFrete: -0.01m));
    }

    [Fact]
    public void Dado_FreteZero_Quando_CriarPedido_Entao_DeveConstruir()
    {
        var pedido = CriarPedido(valorDoFrete: 0m);

        Assert.Equal(0m, pedido.ValorDoFrete);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Dado_TransportadoraEmBranco_Quando_CriarPedido_Entao_DeveLancarArgumentException(string? transportadora)
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(transportadora: transportadora!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Dado_ServicoEmBranco_Quando_CriarPedido_Entao_DeveLancarArgumentException(string? servico)
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(servico: servico!));
    }

    [Fact]
    public void Dado_PrazoMinimoZeroOuNegativo_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(prazoMinimoEmDias: 0));
    }

    [Fact]
    public void Dado_PrazoMaximoMenorQueOMinimo_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarPedido(prazoMinimoEmDias: 5, prazoMaximoEmDias: 4));
    }

    [Fact]
    public void Dado_PrazoMaximoIgualAoMinimo_Quando_CriarPedido_Entao_DeveConstruir()
    {
        var pedido = CriarPedido(prazoMinimoEmDias: 3, prazoMaximoEmDias: 3);

        Assert.Equal(3, pedido.PrazoMinimoEmDias);
        Assert.Equal(3, pedido.PrazoMaximoEmDias);
    }

    [Fact]
    public void Dado_PedidoNovo_Quando_AcrescentarItem_Entao_DeveAcumularNaColecao()
    {
        var pedido = CriarPedido();

        pedido.AcrescentarItem(Guid.NewGuid(), 2, 9.90m);
        pedido.AcrescentarItem(Guid.NewGuid(), 1, 19.90m);

        Assert.Equal(2, pedido.Itens.Count);
    }

    [Fact]
    public void Dado_ItemComPromocao_Quando_AcrescentarItem_Entao_DeveGuardarDescontoERotulo()
    {
        var pedido = CriarPedido();

        pedido.AcrescentarItem(Guid.NewGuid(), 3, 10m, 10m, "Leve 3, pague 2");

        var item = Assert.Single(pedido.Itens);
        Assert.Equal(10m, item.DescontoDaPromocao);
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
    }

    [Fact]
    public void Dado_PedidoComItens_Quando_LerAColecao_Entao_DeveSerSomenteLeitura()
    {
        var pedido = CriarPedido();
        pedido.AcrescentarItem(Guid.NewGuid(), 1, 9.90m);

        Assert.IsAssignableFrom<IReadOnlyCollection<ItemPedido>>(pedido.Itens);
    }

    [Fact]
    public void Dado_ItemInvalido_Quando_AcrescentarItem_Entao_DeveLancarArgumentException()
    {
        var pedido = CriarPedido();

        Assert.Throws<ArgumentException>(() => pedido.AcrescentarItem(Guid.NewGuid(), 0, 9.90m));
    }

    [Fact]
    public void Dado_UmPedido_Quando_LerNumeroVisivel_Entao_DeveTerOitoCaracteresMaiusculos()
    {
        var pedido = CriarPedido();

        var numero = pedido.NumeroVisivel();

        Assert.Equal(8, numero.Length);
        Assert.Equal(numero.ToUpperInvariant(), numero);
    }

    [Fact]
    public void Dado_OMesmoPedido_Quando_LerNumeroVisivelDuasVezes_Entao_DeveSerEstavel()
    {
        var pedido = CriarPedido();

        Assert.Equal(pedido.NumeroVisivel(), pedido.NumeroVisivel());
    }

    [Fact]
    public void Dado_PedidoConfirmado_Quando_Cancelar_Entao_DeveFicarCancelado()
    {
        var pedido = CriarPedido();

        pedido.Cancelar();

        Assert.Equal(PedidoStatus.Cancelado, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoConfirmado_Quando_EnviarEEntregar_Entao_DeveRefletirCadaSituacao()
    {
        var pedido = CriarPedido();
        Assert.Equal(PedidoStatus.Confirmado, pedido.Status);

        pedido.MarcarComoEnviado();
        Assert.Equal(PedidoStatus.Enviado, pedido.Status);

        pedido.MarcarComoEntregue();
        Assert.Equal(PedidoStatus.Entregue, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoConfirmado_Quando_MarcarComoEntregue_Entao_DeveFicarEntregue()
    {
        var pedido = CriarPedido();

        pedido.MarcarComoEntregue();

        Assert.Equal(PedidoStatus.Entregue, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoPendenteAntigo_Quando_MarcarComoEntregue_Entao_DeveFicarEntregue()
    {
        var pedido = CriarPedido();
        typeof(Pedido).GetProperty(nameof(Pedido.Status))!.SetValue(pedido, PedidoStatus.Pendente);

        pedido.MarcarComoEntregue();

        Assert.Equal(PedidoStatus.Entregue, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoCancelado_Quando_MarcarComoEntregue_Entao_DeveLancarInvalidOperationException()
    {
        var pedido = CriarPedido();
        pedido.Cancelar();

        Assert.Throws<InvalidOperationException>(() => pedido.MarcarComoEntregue());
        Assert.Equal(PedidoStatus.Cancelado, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoJaEntregue_Quando_MarcarComoEntregue_Entao_DeveLancarInvalidOperationException()
    {
        var pedido = CriarPedido();
        pedido.MarcarComoEntregue();

        Assert.Throws<InvalidOperationException>(() => pedido.MarcarComoEntregue());
    }

    private Pedido CriarPedidoComItens()
    {
        var pedido = CriarPedido(valor: 90m, valorDoFrete: 10m);
        pedido.AcrescentarItem(Guid.NewGuid(), 2, 25m);
        pedido.AcrescentarItem(Guid.NewGuid(), 3, 10m);
        return pedido;
    }

    [Fact]
    public void Dado_PedidoSemCupom_Quando_ValorDosProdutos_Entao_DeveSerASomaDasLinhas()
    {
        var pedido = CriarPedidoComItens();

        Assert.Equal(80m, pedido.ValorDosProdutos());
        Assert.Null(pedido.CupomId);
        Assert.Equal(0m, pedido.DescontoDoCupom);
        Assert.Equal(0m, pedido.DescontoNoFrete);
    }

    [Fact]
    public void Dado_CupomAplicado_Quando_ValorDosProdutos_Entao_DeveDescontar()
    {
        var pedido = CriarPedidoComItens();
        var cupomId = Guid.NewGuid();

        pedido.AplicarCupom(cupomId, "BEMVINDO10", 8m, 0m);

        Assert.Equal(72m, pedido.ValorDosProdutos());
        Assert.Equal(cupomId, pedido.CupomId);
        Assert.Equal("BEMVINDO10", pedido.CodigoDoCupom);
        Assert.Equal(8m, pedido.DescontoDoCupom);
    }

    [Fact]
    public void Dado_CupomDeFreteGratis_Quando_AplicarCupom_Entao_DeveGuardarDescontoNoFrete()
    {
        var pedido = CriarPedidoComItens();

        pedido.AplicarCupom(Guid.NewGuid(), "FRETEGRATIS", 0m, 10m);

        Assert.Equal(10m, pedido.DescontoNoFrete);
        Assert.Equal(80m, pedido.ValorDosProdutos());
    }

    [Fact]
    public void Dado_DescontoMaiorQueProdutos_Quando_AplicarCupom_Entao_DeveLancarArgumentException()
    {
        var pedido = CriarPedidoComItens();

        Assert.Throws<ArgumentException>(() => pedido.AplicarCupom(Guid.NewGuid(), "BEMVINDO10", 80.01m, 0m));
    }

    [Fact]
    public void Dado_DescontoNoFreteMaiorQueOFrete_Quando_AplicarCupom_Entao_DeveLancarArgumentException()
    {
        var pedido = CriarPedidoComItens();

        Assert.Throws<ArgumentException>(() => pedido.AplicarCupom(Guid.NewGuid(), "FRETEGRATIS", 0m, 10.01m));
    }

    [Fact]
    public void Dado_DescontoNegativo_Quando_AplicarCupom_Entao_DeveLancarArgumentException()
    {
        var pedido = CriarPedidoComItens();

        Assert.Throws<ArgumentException>(() => pedido.AplicarCupom(Guid.NewGuid(), "BEMVINDO10", -1m, 0m));
    }

    [Fact]
    public void Dado_CupomInvalido_Quando_AplicarCupom_Entao_DeveLancarArgumentException()
    {
        var pedido = CriarPedidoComItens();

        Assert.Throws<ArgumentException>(() => pedido.AplicarCupom(Guid.Empty, "BEMVINDO10", 8m, 0m));
        Assert.ThrowsAny<ArgumentException>(() => pedido.AplicarCupom(Guid.NewGuid(), " ", 8m, 0m));
    }

    private static readonly FaixaDeRetirada _faixaValida =
        new(new DateOnly(2026, 11, 23), new TimeOnly(14, 0), new TimeOnly(15, 0));

    [Fact]
    public void Dado_FaixaValida_Quando_ParaRetirada_Entao_DeveTerModalidadeRetiradaEFreteZero()
    {
        var pedido = Pedido.ParaRetirada(_usuarioValido, 42.50m, _faixaValida);

        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, pedido.Modalidade);
        Assert.Equal(0m, pedido.ValorDoFrete);
        Assert.Equal(42.50m, pedido.Valor);
        Assert.Null(pedido.EnderecoEntregaId);
        Assert.Null(pedido.Transportadora);
        Assert.Null(pedido.Servico);
        Assert.Null(pedido.PrazoMinimoEmDias);
        Assert.Null(pedido.PrazoMaximoEmDias);
        Assert.Equal(new DateOnly(2026, 11, 23), pedido.DataDaRetirada);
        Assert.Equal(new TimeOnly(14, 0), pedido.InicioDaRetirada);
        Assert.Equal(new TimeOnly(15, 0), pedido.FimDaRetirada);
        Assert.Equal(PedidoStatus.Confirmado, pedido.Status);
        Assert.NotEqual(Guid.Empty, pedido.PedidoId);
    }

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPedido_Entao_DeveTerModalidadeEntregaSemRetirada()
    {
        var pedido = CriarPedido();

        Assert.Equal(ModalidadeDeEntrega.Entrega, pedido.Modalidade);
        Assert.Null(pedido.DataDaRetirada);
        Assert.Null(pedido.InicioDaRetirada);
        Assert.Null(pedido.FimDaRetirada);
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_ParaRetirada_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Pedido.ParaRetirada(Guid.Empty, 42.50m, _faixaValida));
    }

    [Fact]
    public void Dado_ValorNegativo_Quando_ParaRetirada_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Pedido.ParaRetirada(_usuarioValido, -1m, _faixaValida));
    }

    [Fact]
    public void Dado_FaixaNula_Quando_ParaRetirada_Entao_DeveLancarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Pedido.ParaRetirada(_usuarioValido, 42.50m, null!));
    }

    [Fact]
    public void Dado_FaixaComFimAntesDoInicio_Quando_ParaRetirada_Entao_DeveLancarArgumentException()
    {
        var faixa = new FaixaDeRetirada(new DateOnly(2026, 11, 23), new TimeOnly(15, 0), new TimeOnly(14, 0));

        Assert.Throws<ArgumentException>(() => Pedido.ParaRetirada(_usuarioValido, 42.50m, faixa));
    }

    [Fact]
    public void Dado_PedidoDeRetirada_Quando_AplicarCupomComDescontoNoFrete_Entao_DeveLancarArgumentException()
    {
        var pedido = Pedido.ParaRetirada(_usuarioValido, 42.50m, _faixaValida);
        pedido.AcrescentarItem(Guid.NewGuid(), 1, 42.50m);

        Assert.Throws<ArgumentException>(() => pedido.AplicarCupom(Guid.NewGuid(), "FRETEGRATIS", 0m, 1m));
    }
}
