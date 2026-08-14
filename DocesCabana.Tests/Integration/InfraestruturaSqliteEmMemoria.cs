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
    /// Persiste as duas metades de um usuário válido — a ContaDeAcesso e o
    /// Usuario do domínio, com o mesmo Guid — e devolve o identificador
    /// compartilhado, para testes que precisam de uma entidade com FK real
    /// para Usuario (Endereco, Favorito, Avaliacao, Pedido).
    /// </summary>
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

    /// <summary>
    /// Persiste uma avaliação. <paramref name="dataCriacao"/> permite
    /// controlar a data para testes de ordenação — sem isso, avaliações
    /// criadas na mesma passagem de teste podem cair no mesmo instante e
    /// tornar a ordenação por "mais recentes" indeterminística. O construtor
    /// público não expõe esse parâmetro de propósito (RN-09: a data é sempre
    /// "agora"); aqui é reflection, o mesmo backdoor de teste já usado em
    /// outras entidades desta base.
    /// </summary>
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
