namespace DocesCabana.Domain.Entities;

/// <summary>
/// Registra que um usuário marcou uma avaliação como útil. Chave composta
/// (AvaliacaoId, UsuarioId): uma pessoa vota no máximo uma vez por avaliação
/// (RN-06), garantido no banco pela chave, não só no código.
/// </summary>
public class VotoUtil
{
    public Guid AvaliacaoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    // Navegação filho -> pai, anulável (vem null sem Include).
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
