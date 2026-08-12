namespace DocesCabana.Domain.Entities;

public class Favorito
{
    public Guid ProdutoId { get; private set; }

    // Sem navegação: Usuario vive na Infrastructure (RQ-02 da spec 003).
    public Guid UsuarioId { get; private set; }

    // Navegação filho -> pai. Ambas as pontas do produto vivem no domínio.
    public Produto? Produto { get; private set; }

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
