using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

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
    public void Dado_DadosValidos_Quando_CriarPedido_Entao_DeveNascerPendenteComPagamentoNaoAprovado()
    {
        var pedido = CriarPedido(valor: 50.00m);

        Assert.Equal(PedidoStatus.Pendente, pedido.Status);
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

    // Métodos usados só pela semeadura (DbInitializer, spec 022) — nenhum
    // caminho real desta entrega os chama (spec §10).
    [Fact]
    public void Dado_PedidoPendente_Quando_Cancelar_Entao_DeveFicarCancelado()
    {
        var pedido = CriarPedido();

        pedido.Cancelar();

        Assert.Equal(PedidoStatus.Cancelado, pedido.Status);
    }

    [Fact]
    public void Dado_PedidoPendente_Quando_ConfirmarEnviarEEntregar_Entao_DeveRefletirCadaSituacao()
    {
        var pedido = CriarPedido();

        pedido.Confirmar();
        Assert.Equal(PedidoStatus.Confirmado, pedido.Status);

        pedido.MarcarComoEnviado();
        Assert.Equal(PedidoStatus.Enviado, pedido.Status);

        pedido.MarcarComoEntregue();
        Assert.Equal(PedidoStatus.Entregue, pedido.Status);
    }
}
