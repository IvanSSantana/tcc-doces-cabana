namespace DocesCabana.Domain.Entities;

public class ItemCarrinho
{
    public const short QuantidadeMinima = 1;
    public const short QuantidadeMaxima = 99;

    public Guid UsuarioId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public short Quantidade { get; private set; }

    public Produto? Produto { get; private set; }

    public Usuario? Usuario { get; private set; }

    protected ItemCarrinho() { }

    public ItemCarrinho(Guid usuarioId, Guid produtoId, short quantidade = QuantidadeMinima)
    {
        ValidarUsuario(usuarioId);
        ValidarProduto(produtoId);
        ValidarQuantidade(quantidade);

        UsuarioId = usuarioId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
    }

    public void AlterarQuantidade(short quantidade)
    {
        ValidarQuantidade(quantidade);

        Quantidade = quantidade;
    }

    public void Acrescentar(short quantidade)
    {
        ValidarQuantidade(quantidade);

        var soma = Quantidade + quantidade;
        Quantidade = (short)Math.Min(soma, QuantidadeMaxima);
    }

    private static void ValidarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }

    private static void ValidarProduto(Guid produtoId)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));
    }

    private static void ValidarQuantidade(short quantidade)
    {
        if (quantidade < QuantidadeMinima || quantidade > QuantidadeMaxima)
            throw new ArgumentException(
                $"Quantidade deve estar entre {QuantidadeMinima} e {QuantidadeMaxima}.", nameof(quantidade));
    }
}
