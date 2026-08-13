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
        // exatamente o comportamento que a transação explícita removida (RQ-02
        // da spec 002) dava e que o SaveChangesAsync já fornece por si.
        var conta1 = new ContaDeAcesso("cliente.um@teste.com");
        var conta2 = new ContaDeAcesso("cliente.dois@teste.com");
        await Contexto.Users.AddAsync(conta1);
        await Contexto.Users.AddAsync(conta2);
        await Contexto.SaveChangesAsync();

        var usuario1 = new Usuario(conta1.Id, "Cliente Um", "52998224725", "11987654321", new DateTime(1990, 1, 1));
        var usuario2 = new Usuario(conta2.Id, "Cliente Dois", "52998224725", "11987654322", new DateTime(1991, 2, 2));

        await Contexto.Produtos.AddAsync(produtoValido);
        await Contexto.Usuarios.AddAsync(usuario1);
        await Contexto.Usuarios.AddAsync(usuario2);

        await Assert.ThrowsAsync<DbUpdateException>(() => uow.SalvarAlteracoes());

        var produtoNoBanco = await Contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == produtoValido.ProdutoId);
        Assert.Null(produtoNoBanco);
    }
}
