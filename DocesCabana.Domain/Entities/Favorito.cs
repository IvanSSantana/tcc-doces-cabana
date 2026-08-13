namespace DocesCabana.Domain.Entities;

public class Favorito
{
    public Guid ProdutoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    // Navegações filho -> pai. Usuario agora é do domínio (spec 004).
    public Produto? Produto { get; private set; }

    public Usuario? Usuario { get; private set; }

    protected Favorito() { }

    public Favorito(Guid produtoId, Guid usuarioId)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));

        ProdutoId = produtoId;
        UsuarioId = usuarioId;
    }
}
