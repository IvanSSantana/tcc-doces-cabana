namespace DocesCabana.Tests.Integration;

// RF-09/RN-05 (spec 028): quando o contêiner Postgres de teste não sobe
// (Docker Desktop desligado, o caso comum), o Testcontainers lança uma
// exceção que descreve o sintoma ("npipe", "daemon") — não a causa nem o
// que fazer. PostgresDeTeste captura essa exceção e relança com esta
// mensagem por cima, preservando a original para quem precisar diagnosticar
// algo diferente de "Docker desligado".
public static class MensagemDePostgresIndisponivel
{
    public static string Montar(Exception causaOriginal) =>
        $"""
        Não foi possível subir o Postgres de teste. O Docker Desktop precisa estar em execução — a suíte sobe um contêiner descartável e não usa banco nenhum da sua máquina nem do Supabase.

        Causa original: {causaOriginal.Message}
        """;
}
