using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration;

public class DatabaseIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarAoRepositorioSemSalvar_Entao_NaoDeveEstarNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Null(produtoNoBanco);
    }

    [Fact]
    public async Task Dado_UmNovoProduto_Quando_AdicionarECommitarPeloUnitOfWork_Entao_DevePersistirNoBanco()
    {
        var repositorio = new Repository<Produto>(Contexto);
        var uow = new UnitOfWork(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var produto = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        await repositorio.Adicionar(produto);
        var salvos = await uow.SalvarAlteracoes();

        Assert.True(salvos > 0);
        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.NotNull(produtoNoBanco);
        Assert.Equal("Bolo de Cenoura", produtoNoBanco.Nome);
    }

    [Fact]
    public async Task Dado_DuasAlteracoesUmaInvalidaParaOBanco_Quando_SalvarAlteracoes_Entao_NenhumaDevePersistir()
    {
        var uow = new UnitOfWork(Contexto);
        var subcategoriaId = await SemearSubcategoria();
        var produtoValido = new Produto(subcategoriaId, "Bolo de Cenoura", 12.00m, "https://imagem.com/bolo.jpg");

        // Dois usuários com o mesmo CPF violam o índice único da tabela — o
        // SalvarAlteracoes falha, e nenhuma das duas alterações deve persistir,
        // exatamente o comportamento que a transação explícita removida (RQ-02)
        // dava e que o SaveChangesAsync já fornece por si.
        var usuario1 = new Usuario("Cliente Um", "cliente.um@teste.com", "11987654321", new DateTime(1990, 1, 1), "52998224725");
        var usuario2 = new Usuario("Cliente Dois", "cliente.dois@teste.com", "11987654322", new DateTime(1991, 2, 2), "52998224725");

        await Contexto.Produtos.AddAsync(produtoValido);
        await Contexto.Users.AddAsync(usuario1);
        await Contexto.Users.AddAsync(usuario2);

        await Assert.ThrowsAsync<DbUpdateException>(() => uow.SalvarAlteracoes());

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produtoValido.ProdutoId);
        Assert.Null(produtoNoBanco);
    }
}
