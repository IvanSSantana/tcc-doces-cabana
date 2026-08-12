using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class PedidoTests
{
    private readonly Guid _usuarioValido = Guid.NewGuid();
    private readonly Guid _enderecoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPedido_Entao_DeveNascerPendenteComPagamentoNaoAprovado()
    {
        var pedido = new Pedido(_usuarioValido, _enderecoValido, 50.00m);

        Assert.Equal(PedidoStatus.Pendente, pedido.Status);
        Assert.False(pedido.PagamentoAprovado);
        Assert.Equal(50.00m, pedido.Valor);
        Assert.True((DateTime.UtcNow - pedido.Data) < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pedido(Guid.Empty, _enderecoValido, 50.00m));
    }

    [Fact]
    public void Dado_EnderecoInvalido_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pedido(_usuarioValido, Guid.Empty, 50.00m));
    }

    [Fact]
    public void Dado_ValorNegativo_Quando_CriarPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pedido(_usuarioValido, _enderecoValido, -1m));
    }

    [Fact]
    public void Dado_ValorZero_Quando_CriarPedido_Entao_DeveConstruir()
    {
        var pedido = new Pedido(_usuarioValido, _enderecoValido, 0m);

        Assert.Equal(0m, pedido.Valor);
    }
}
