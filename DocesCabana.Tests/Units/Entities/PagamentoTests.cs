using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class PagamentoTests
{
    private readonly Guid _pedidoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPagamento_Entao_DeveNascerPendenteSemDataDePagamento()
    {
        var pagamento = new Pagamento(_pedidoValido, MetodoPagamento.Pix, 50.00m);

        Assert.Equal(PagamentoStatus.Pendente, pagamento.Status);
        Assert.Null(pagamento.DataPagamento);
        Assert.Equal(50.00m, pagamento.Valor);
    }

    [Fact]
    public void Dado_PedidoInvalido_Quando_CriarPagamento_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Pagamento(Guid.Empty, MetodoPagamento.Pix, 50.00m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_ValorInvalido_Quando_CriarPagamento_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() => new Pagamento(_pedidoValido, MetodoPagamento.Pix, valor));
    }
}
