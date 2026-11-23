using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Tests.Units.Mappings;

public class PedidoMapperTests
{
    private static Pedido CriarPedidoComPromocao()
    {
        var pedido = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 28m, 8m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(Guid.NewGuid(), 3, 10m, 10m, "Leve 3, pague 2");
        return pedido;
    }

    [Fact]
    public void Dado_ItemComPromocao_Quando_ToConfirmacaoDTO_Entao_LinhaDeveSerLiquidaComRotulo()
    {
        var confirmacao = PedidoMapper.ToConfirmacaoDTO(CriarPedidoComPromocao(), MetodoPagamento.Pix);

        var item = Assert.Single(confirmacao.Itens);
        Assert.Equal(20m, item.ValorDaLinha);
        Assert.Equal(10m, item.DescontoDaPromocao);
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
        Assert.Equal(20m, confirmacao.ValorDosProdutos);
    }

    [Fact]
    public void Dado_ItemComPromocao_Quando_ToDetalheDTO_Entao_LinhaDeveSerLiquidaComRotulo()
    {
        var detalhe = PedidoMapper.ToDetalheDTO(CriarPedidoComPromocao(), null);

        var item = Assert.Single(detalhe.Itens);
        Assert.Equal(20m, item.ValorDaLinha);
        Assert.Equal(10m, item.DescontoDaPromocao);
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
    }

    private static Pedido CriarPedidoDeRetirada()
    {
        var faixa = new FaixaDeRetirada(new DateOnly(2026, 11, 23), new TimeOnly(14, 0), new TimeOnly(15, 0));
        var pedido = Pedido.ParaRetirada(Guid.NewGuid(), 20m, faixa);
        pedido.AcrescentarItem(Guid.NewGuid(), 2, 10m);
        return pedido;
    }

    [Fact]
    public void Dado_PedidoDeRetirada_Quando_ToConfirmacaoDTO_Entao_DeveTrazerModalidadeEFaixa()
    {
        var confirmacao = PedidoMapper.ToConfirmacaoDTO(CriarPedidoDeRetirada(), MetodoPagamento.Pix);

        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, confirmacao.Modalidade);
        Assert.Equal(new DateOnly(2026, 11, 23), confirmacao.DataDaRetirada);
        Assert.Equal(new TimeOnly(14, 0), confirmacao.InicioDaRetirada);
        Assert.Equal(new TimeOnly(15, 0), confirmacao.FimDaRetirada);
        Assert.Null(confirmacao.Transportadora);
        Assert.Equal(0m, confirmacao.ValorDoFrete);
        Assert.Equal(20m, confirmacao.ValorTotal);
    }

    [Fact]
    public void Dado_PedidoDeRetirada_Quando_ToDetalheDTO_Entao_DeveTrazerModalidadeEFaixa()
    {
        var detalhe = PedidoMapper.ToDetalheDTO(CriarPedidoDeRetirada(), null);

        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, detalhe.Modalidade);
        Assert.Equal(new DateOnly(2026, 11, 23), detalhe.DataDaRetirada);
        Assert.Equal(new TimeOnly(14, 0), detalhe.InicioDaRetirada);
        Assert.Equal(new TimeOnly(15, 0), detalhe.FimDaRetirada);
        Assert.Null(detalhe.PrazoMinimoEmDias);
    }

    [Fact]
    public void Dado_PedidoDeEntrega_Quando_ToDetalheDTO_Entao_DeveTrazerModalidadeEntrega()
    {
        var detalhe = PedidoMapper.ToDetalheDTO(CriarPedidoComPromocao(), null);

        Assert.Equal(ModalidadeDeEntrega.Entrega, detalhe.Modalidade);
        Assert.Null(detalhe.DataDaRetirada);
        Assert.Equal("Correios", detalhe.Transportadora);
    }

    [Theory]
    [InlineData(PedidoStatus.Pendente, true)]
    [InlineData(PedidoStatus.Confirmado, true)]
    [InlineData(PedidoStatus.Enviado, true)]
    [InlineData(PedidoStatus.Entregue, false)]
    [InlineData(PedidoStatus.Cancelado, false)]
    public void Dado_PedidoNaSituacao_Quando_ToResumoDTO_Entao_PodeMarcarComoEntregueDeveRefletirASituacao(
        PedidoStatus situacao, bool esperado)
    {
        var pedido = CriarPedidoComPromocao();
        typeof(Pedido).GetProperty(nameof(Pedido.Status))!.SetValue(pedido, situacao);

        var resumo = PedidoMapper.ToResumoDTO(pedido);

        Assert.Equal(esperado, resumo.PodeMarcarComoEntregue);
    }
}
