namespace DocesCabana.Domain.Entities;

public class Estoque
{
    public Guid ProdutoId { get; private set; }

    public short Quantidade { get; private set; }

    // Navegação filho -> pai. Chave compartilhada com Produto (1:1).
    public Produto? Produto { get; private set; }

    protected Estoque() { }

    public Estoque(Guid produtoId, short quantidade)
    {
        ValidarProduto(produtoId);
        ValidarQuantidade(quantidade);

        ProdutoId = produtoId;
        Quantidade = quantidade;
    }

    public void Adicionar(short quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade a adicionar deve ser maior que zero.", nameof(quantidade));

        Quantidade += quantidade;
    }

    public void Retirar(short quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade a retirar deve ser maior que zero.", nameof(quantidade));

        if (quantidade > Quantidade)
            throw new InvalidOperationException("Quantidade em estoque insuficiente.");

        Quantidade -= quantidade;
    }

    private void ValidarProduto(Guid produtoId)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));
    }

    private void ValidarQuantidade(short quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("Quantidade não pode ser negativa.", nameof(quantidade));
    }
}
