using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

public class Pagamento
{
    public Guid PagamentoId { get; private set; }

    public Guid PedidoId { get; private set; }

    public MetodoPagamento Metodo { get; private set; }

    public PagamentoStatus Status { get; private set; }

    public decimal Valor { get; private set; }

    public DateTime? DataPagamento { get; private set; }

    // Navegação filho -> pai. Chave compartilhada com Pedido (1:1).
    public Pedido? Pedido { get; private set; }

    protected Pagamento() { }

    public Pagamento(Guid pedidoId, MetodoPagamento metodo, decimal valor, Guid id = default)
    {
        ValidarPedido(pedidoId);
        ValidarValor(valor);

        PagamentoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        PedidoId = pedidoId;
        Metodo = metodo;
        Valor = valor;
        Status = PagamentoStatus.Pendente;
        DataPagamento = null;
    }

    private void ValidarPedido(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
            throw new ArgumentException("Pedido inválido.", nameof(pedidoId));
    }

    private void ValidarValor(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));
    }
}
