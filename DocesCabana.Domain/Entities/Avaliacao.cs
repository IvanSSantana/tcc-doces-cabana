namespace DocesCabana.Domain.Entities;

public class Avaliacao
{
    public Guid AvaliacaoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public string? Comentario { get; private set; }

    public byte Nota { get; private set; }

    public DateTime DataCriacao { get; private set; }

    private readonly List<VotoUtil> _votos = [];

    public IReadOnlyCollection<VotoUtil> Votos => _votos.AsReadOnly();

    public int TotalUteis => _votos.Select(v => v.UsuarioId).Distinct().Count();

    public Produto? Produto { get; private set; }

    public Usuario? Usuario { get; private set; }

    protected Avaliacao() { }

    public Avaliacao(
        Guid usuarioId,
        Guid produtoId,
        byte nota,
        string? comentario = null,
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
        DataCriacao = DateTime.UtcNow;
    }

    public bool MarcadaComoUtilPor(Guid usuarioId) =>
        _votos.Any(v => v.UsuarioId == usuarioId);

    public bool AlternarVotoUtil(Guid usuarioId)
    {
        if (usuarioId == UsuarioId)
            throw new InvalidOperationException("Você não pode marcar como útil a própria avaliação.");

        var votoExistente = _votos.FirstOrDefault(v => v.UsuarioId == usuarioId);
        if (votoExistente is not null)
        {
            _votos.Remove(votoExistente);
            return false;
        }

        _votos.Add(new VotoUtil(AvaliacaoId, usuarioId));
        return true;
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

    private static void ValidarNota(byte nota)
    {
        if (nota < 1 || nota > 5)
            throw new ArgumentException("Nota deve estar entre 1 e 5.", nameof(nota));
    }

    private static void ValidarComentario(string? comentario)
    {
        if (comentario is not null && comentario.Length > 255)
            throw new ArgumentException("Comentário deve ter no máximo 255 caracteres.", nameof(comentario));
    }
}
