using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration;

public abstract class InfraestruturaSqliteEmMemoria : IAsyncLifetime
{
    private SqliteConnection _conexao = null!;
    protected DocesCabanaDbContext Contexto = null!;

    public async Task InitializeAsync()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        await _conexao.OpenAsync();

        var opcoes = new DbContextOptionsBuilder<DocesCabanaDbContext>()
            .UseSqlite(_conexao)
            .Options;

        Contexto = new DocesCabanaDbContext(opcoes);
        await Contexto.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Contexto.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}
