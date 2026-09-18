namespace DocesCabana.Domain.Entities;

public class VotoUtil
{
    public Guid AvaliacaoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Avaliacao? Avaliacao { get; private set; }

    protected VotoUtil() { }

    public VotoUtil(Guid avaliacaoId, Guid usuarioId)
    {
        ValidarAvaliacao(avaliacaoId);
        ValidarUsuario(usuarioId);

        AvaliacaoId = avaliacaoId;
        UsuarioId = usuarioId;
    }

    private static void ValidarAvaliacao(Guid avaliacaoId)
    {
        if (avaliacaoId == Guid.Empty)
            throw new ArgumentException("Avaliação inválida.", nameof(avaliacaoId));
    }

    private static void ValidarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }
}
