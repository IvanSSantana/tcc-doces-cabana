using Npgsql;
using Testcontainers.PostgreSql;

namespace DocesCabana.Tests.Integration.Infraestrutura;

internal static class PostgresDeTeste
{
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
                try
                {
                    var conteiner = new PostgreSqlBuilder(Imagem).Build();
                    await conteiner.StartAsync();
                    _conteiner = conteiner;
                }
                catch (Exception causaOriginal)
                {
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

        // Sem pool: cada teste usa um banco próprio, e o pool deixaria uma conexão ociosa aberta por
        // banco até estourar o max_connections (100) do contêiner — "53300: too many clients".
        var construtor = new NpgsqlConnectionStringBuilder(conexaoBase) { Database = nomeDoBanco, Pooling = false };
        return construtor.ConnectionString;
    }
}
