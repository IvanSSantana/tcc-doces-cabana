namespace DocesCabana.Domain.Entities;

public class ItemPedido
{
    public Guid ItemPedidoId { get; private set; }

    public Guid PedidoId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public short Quantidade { get; private set; }

    public decimal PrecoUnitario { get; private set; }

    public decimal DescontoDaPromocao { get; private set; }

    public string? RotuloDaPromocao { get; private set; }

    public Pedido? Pedido { get; private set; }

    public Produto? Produto { get; private set; }

    protected ItemPedido() { }

    public ItemPedido(
        Guid pedidoId,
        Guid produtoId,
        short quantidade,
        decimal precoUnitario,
        decimal descontoDaPromocao = 0m,
        string? rotuloDaPromocao = null,
        Guid id = default)
    {
        ValidarPedido(pedidoId);
        ValidarProduto(produtoId);
        ValidarQuantidade(quantidade);
        ValidarPrecoUnitario(precoUnitario);
        ValidarDesconto(descontoDaPromocao, precoUnitario * quantidade);
        ValidarRotulo(rotuloDaPromocao);

        ItemPedidoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        PedidoId = pedidoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        DescontoDaPromocao = descontoDaPromocao;
        RotuloDaPromocao = rotuloDaPromocao;
    }

    public decimal CalcularValorDaLinha() => PrecoUnitario * Quantidade - DescontoDaPromocao;

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

    private static void ValidarDesconto(decimal desconto, decimal valorBruto)
    {
        if (desconto < 0)
            throw new ArgumentException("Desconto não pode ser negativo.", nameof(desconto));

        if (desconto >= valorBruto)
            throw new ArgumentException("Desconto deve ser menor que o valor do item.", nameof(desconto));
    }

    private static void ValidarRotulo(string? rotulo)
    {
        if (rotulo is not null && rotulo.Length > 100)
            throw new ArgumentException("Rótulo da promoção deve ter no máximo 100 caracteres.", nameof(rotulo));
    }
}
