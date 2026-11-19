using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

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
}
