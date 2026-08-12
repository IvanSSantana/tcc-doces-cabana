using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Identity;
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

    /// <summary>
    /// Persiste uma categoria e uma subcategoria válidas e devolve o
    /// SubcategoriaId, para testes que precisam de um produto com chave
    /// estrangeira real (a FK de Produto.SubcategoriaId passou a ser
    /// enforçada a partir da spec 003).
    /// </summary>
    protected async Task<Guid> SemearSubcategoria()
    {
        var categoria = new Categoria("Doces");
        var subcategoria = new Subcategoria(categoria.CategoriaId, "Doces de Tacho");

        Contexto.Categorias.Add(categoria);
        Contexto.Subcategorias.Add(subcategoria);
        await Contexto.SaveChangesAsync();

        return subcategoria.SubcategoriaId;
    }

    /// <summary>
    /// Persiste um usuário válido e devolve o Id, para testes que precisam de
    /// uma entidade com FK real para Usuario (Endereco, Favorito, Avaliacao,
    /// Pedido).
    /// </summary>
    protected async Task<Guid> SemearUsuario()
    {
        var usuario = new Usuario(
            "Cliente Teste",
            $"{Guid.NewGuid():N}@teste.com",
            "11987654321",
            new DateTime(1990, 1, 1),
            "52998224725");

        Contexto.Users.Add(usuario);
        await Contexto.SaveChangesAsync();

        return usuario.Id;
    }
}
