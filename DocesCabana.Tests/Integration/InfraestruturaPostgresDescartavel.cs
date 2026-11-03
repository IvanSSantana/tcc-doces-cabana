using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration;

public abstract class InfraestruturaPostgresDescartavel : IAsyncLifetime
{
    protected DocesCabanaDbContext Contexto = null!;

    public async Task InitializeAsync()
    {
        var connectionString = await PostgresDeTeste.CriarBancoNovo();

        var opcoes = new DbContextOptionsBuilder<DocesCabanaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        Contexto = new DocesCabanaDbContext(opcoes);
        await Contexto.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await Contexto.DisposeAsync();

    protected async Task<Guid> SemearSubcategoria()
    {
        var categoria = new Categoria("Doces");
        var subcategoria = new Subcategoria(categoria.CategoriaId, "Doces de Tacho");

        Contexto.Categorias.Add(categoria);
        Contexto.Subcategorias.Add(subcategoria);
        await Contexto.SaveChangesAsync();

        return subcategoria.SubcategoriaId;
    }

    protected async Task<Guid> SemearUsuario(string nome = "Cliente Teste", string cpf = "52998224725")
    {
        var conta = new ContaDeAcesso($"{Guid.NewGuid():N}@teste.com");
        Contexto.Users.Add(conta);
        await Contexto.SaveChangesAsync();

        var usuario = new Usuario(conta.Id, nome, cpf, "11987654321", new DateTime(1990, 1, 1));
        Contexto.Usuarios.Add(usuario);
        await Contexto.SaveChangesAsync();

        return conta.Id;
    }

    protected async Task<Avaliacao> SemearAvaliacao(
        Guid produtoId, Guid usuarioId, byte nota, string? comentario = null, DateTime? dataCriacao = null)
    {
        var avaliacao = new Avaliacao(usuarioId, produtoId, nota, comentario);

        if (dataCriacao.HasValue)
        {
            typeof(Avaliacao).GetProperty(nameof(Avaliacao.DataCriacao))!.SetValue(avaliacao, dataCriacao.Value);
        }

        Contexto.Avaliacoes.Add(avaliacao);
        await Contexto.SaveChangesAsync();

        return avaliacao;
    }
}
