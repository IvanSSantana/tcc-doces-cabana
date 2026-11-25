namespace DocesCabana.Domain.Constants;

/// <summary>
/// O admin principal é o admin semeado: só ele desativa e reativa outros administradores (spec 039, D1).
/// </summary>
public static class AdministradorPrincipal
{
    public const string Email = "admin@docescabana.com.br";

    /// <summary>
    /// Fim do lockout gravado ao desativar uma conta. Uma conta está desativada quando o fim do
    /// lockout chega a esta data; o bloqueio de 15 minutos por senha errada nunca chega (D4).
    /// Não é <see cref="DateTimeOffset.MaxValue"/>: o Postgres guarda microssegundos e o valor
    /// voltaria truncado.
    /// </summary>
    public static readonly DateTimeOffset FimDaDesativacao = new(9999, 12, 1, 0, 0, 0, TimeSpan.Zero);
}
