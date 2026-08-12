namespace DocesCabana.Domain.Entities;

public class Avaliacao
{
    public Guid AvaliacaoId { get; private set; }

    // Sem navegação: Usuario vive na Infrastructure (RQ-02 da spec 003).
    public Guid UsuarioId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public string? Comentario { get; private set; }

    public byte Nota { get; private set; }

    public bool UpVote { get; private set; }

    // Navegação filho -> pai.
    public Produto? Produto { get; private set; }

    protected Avaliacao() { }

    public Avaliacao(
        Guid usuarioId,
        Guid produtoId,
        byte nota,
        string? comentario = null,
        bool upVote = false,
        Guid id = default)
    {
        ValidarUsuario(usuarioId);
        ValidarProduto(produtoId);
        ValidarNota(nota);
        ValidarComentario(comentario);

        AvaliacaoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        UsuarioId = usuarioId;
        ProdutoId = produtoId;
        Nota = nota;
        Comentario = comentario;
        UpVote = upVote;
    }

    private void ValidarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }

    private void ValidarProduto(Guid produtoId)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));
    }

    private void ValidarNota(byte nota)
    {
        if (nota < 1 || nota > 5)
            throw new ArgumentException("Nota deve estar entre 1 e 5.", nameof(nota));
    }

    private void ValidarComentario(string? comentario)
    {
        if (comentario is not null && comentario.Length > 255)
            throw new ArgumentException("Comentário deve ter no máximo 255 caracteres.", nameof(comentario));
    }
}
