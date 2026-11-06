namespace DocesCabana.Tests.Integration.Infraestrutura;

public static class MensagemDePostgresIndisponivel
{
    public static string Montar(Exception causaOriginal) =>
        $"""
        Não foi possível subir o Postgres de teste. O Docker Desktop precisa estar em execução — a suíte sobe um contêiner descartável e não usa banco nenhum da sua máquina nem do Supabase.

        Causa original: {causaOriginal.Message}
        """;
}
