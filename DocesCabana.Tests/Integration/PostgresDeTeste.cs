using Npgsql;
using Testcontainers.PostgreSql;

namespace DocesCabana.Tests.Integration;

// Contêiner Postgres descartável, compartilhado pela suíte de integração
// inteira — um só para os 60 testes, não um por teste (spec 028, plano §5).
//
// Não é um ICollectionFixture do xUnit, e essa é uma decisão deliberada: usar
// [Collection]/injeção de construtor obrigaria as 60 classes de teste
// existentes a mudar de assinatura, contra o que o plano promete em §5 —
// "nenhum dos 60 testes precisa ser reescrito". Em vez disso, o contêiner é
// um singleton estático, iniciado sob demanda na primeira vez que qualquer
// teste chama InitializeAsync (via InfraestruturaPostgresDescartavel),
// protegido por semáforo contra corrida entre classes de teste rodando em
// paralelo. O Ryuk do próprio Testcontainers derruba o contêiner ao fim do
// processo — não há Dispose explícito aqui de propósito.
internal static class PostgresDeTeste
{
    // Mesma versão maior que o Supabase roda (PostgreSQL 17, medido na T003
    // do plano) — sem isso a suíte valida um motor e a loja roda outro, que
    // é o próprio defeito que esta entrega existe para fechar.
    private const string Imagem = "postgres:17-alpine";

    private static readonly SemaphoreSlim Trava = new(1, 1);
    private static PostgreSqlContainer? _conteiner;

    public static async Task<string> ObterConexaoBase()
    {
        if (_conteiner is not null)
            return _conteiner.GetConnectionString();

        await Trava.WaitAsync();
        try
        {
            if (_conteiner is null)
            {
                // Achado ao rodar com o Docker desligado de verdade: o
                // Testcontainers valida o endpoint do Docker já no Build(),
                // não só no StartAsync() — os dois precisam estar cobertos,
                // senão o try/catch some no caso mais comum (RF-09/RN-05).
                try
                {
                    var conteiner = new PostgreSqlBuilder(Imagem).Build();
                    await conteiner.StartAsync();
                    _conteiner = conteiner;
                }
                catch (Exception causaOriginal)
                {
                    // RF-09/RN-05 (spec 028): o erro cru do Testcontainers
                    // descreve o sintoma (npipe, daemon), não a causa. Quem
                    // roda a suíte precisa saber o que ligar.
                    throw new InvalidOperationException(MensagemDePostgresIndisponivel.Montar(causaOriginal));
                }
            }
        }
        finally
        {
            Trava.Release();
        }

        return _conteiner.GetConnectionString();
    }

    // Um banco novo por teste dentro do contêiner compartilhado — preserva
    // exatamente o isolamento que o SQLite em memória já dava (RF-08, CA-06).
    public static async Task<string> CriarBancoNovo()
    {
        var conexaoBase = await ObterConexaoBase();
        var nomeDoBanco = $"teste_{Guid.NewGuid():N}";

        await using (var admin = new NpgsqlConnection(conexaoBase))
        {
            await admin.OpenAsync();
            await using var comando = admin.CreateCommand();
            comando.CommandText = $"""CREATE DATABASE "{nomeDoBanco}";""";
            await comando.ExecuteNonQueryAsync();
        }

        var construtor = new NpgsqlConnectionStringBuilder(conexaoBase) { Database = nomeDoBanco };
        return construtor.ConnectionString;
    }
}
