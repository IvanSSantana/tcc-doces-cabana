namespace DocesCabana.Domain.Entities;

public class ItemPedido
{
    public Guid ItemPedidoId { get; private set; }

    public Guid PedidoId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public short Quantidade { get; private set; }

    public decimal PrecoUnitario { get; private set; }

    // Navegações filho -> pai.
    public Pedido? Pedido { get; private set; }

    public Produto? Produto { get; private set; }

    protected ItemPedido() { }

    public ItemPedido(Guid pedidoId, Guid produtoId, short quantidade, decimal precoUnitario, Guid id = default)
    {
        ValidarPedido(pedidoId);
        ValidarProduto(produtoId);
        ValidarQuantidade(quantidade);
        ValidarPrecoUnitario(precoUnitario);

        ItemPedidoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        PedidoId = pedidoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    private void ValidarPedido(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
            throw new ArgumentException("Pedido inválido.", nameof(pedidoId));
    }

    private void ValidarProduto(Guid produtoId)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));
    }

    private void ValidarQuantidade(short quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
    }

    private void ValidarPrecoUnitario(decimal precoUnitario)
    {
        if (precoUnitario <= 0)
            throw new ArgumentException("Preço unitário deve ser maior que zero.", nameof(precoUnitario));
    }
}
