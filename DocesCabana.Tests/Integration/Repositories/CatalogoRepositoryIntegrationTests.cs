using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class CatalogoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_ProdutosDeDuasSubcategorias_Quando_FiltrarPorCategoria_Entao_DeveTrazerSoOsDaCategoria()
    {
        var (categoriaDocesId, subBarrasId, subPotesId) = await SemearCategoriaDoces();
        var (_, subVinhosId) = await SemearCategoriaAdega();

        await SemearProduto(subBarrasId, "Barra de Doce", 10m);
        await SemearProduto(subPotesId, "Pote de Doce", 12m);
        await SemearProduto(subVinhosId, "Vinho", 40m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaDocesId, [], false, OrdenacaoCatalogo.NomeAZ);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(2, pagina.Count);
        Assert.All(pagina, p => Assert.Contains(p.SubcategoriaId, new[] { subBarrasId, subPotesId }));
    }

    [Fact]
    public async Task Dado_DuasSubcategoriasMarcadas_Quando_Filtrar_Entao_DeveSomarOsProdutosDasDuas()
    {
        var (categoriaId, subBarrasId, subPotesId) = await SemearCategoriaDoces();
        var produtoBarras = await SemearProduto(subBarrasId, "Barra", 10m);
        var produtoPotes = await SemearProduto(subPotesId, "Pote", 12m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [subBarrasId, subPotesId], false, OrdenacaoCatalogo.NomeAZ);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(2, pagina.Count);
        Assert.Contains(pagina, p => p.ProdutoId == produtoBarras.ProdutoId);
        Assert.Contains(pagina, p => p.ProdutoId == produtoPotes.ProdutoId);
    }

    [Fact]
    public async Task Dado_ProdutosMarcadosESemMarcar_Quando_FiltrarPorSemAcucar_Entao_DeveTrazerSoOsMarcados()
    {
        var (categoriaId, subBarrasId, _) = await SemearCategoriaDoces();
        var comAcucar = await SemearProduto(subBarrasId, "Barra Comum", 10m, semAcucar: false);
        var semAcucar = await SemearProduto(subBarrasId, "Barra Diet", 11m, semAcucar: true);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], true, OrdenacaoCatalogo.NomeAZ);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Single(pagina);
        Assert.Equal(semAcucar.ProdutoId, pagina[0].ProdutoId);
        Assert.DoesNotContain(pagina, p => p.ProdutoId == comAcucar.ProdutoId);
    }

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_BuscarPaginaDoCatalogo_Entao_NaoDeveTraze_lo()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Ativo", 10m);
        await SemearProduto(subId, "Inativo", 10m, status: ProdutoStatus.Inativo);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Single(pagina);
        Assert.Equal("Ativo", pagina[0].Nome);
    }

    [Fact]
    public async Task Dado_ProdutosComPrecosDiferentes_Quando_OrdenarPorMenorPreco_Entao_DeveOrdenarCrescente()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Caro", 30m);
        await SemearProduto(subId, "Barato", 5m);
        await SemearProduto(subId, "Medio", 15m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MenorPreco);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(["Barato", "Medio", "Caro"], pagina.Select(p => p.Nome));
    }

    [Fact]
    public async Task Dado_ProdutosComPrecosDiferentes_Quando_OrdenarPorMaiorPreco_Entao_DeveOrdenarDecrescente()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Caro", 30m);
        await SemearProduto(subId, "Barato", 5m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MaiorPreco);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(["Caro", "Barato"], pagina.Select(p => p.Nome));
    }

    [Fact]
    public async Task Dado_ProdutoComAvaliacaoEProdutoSemAvaliacao_Quando_OrdenarPorMelhorAvaliados_Entao_ProdutoSemNotaDeveIrParaOFim()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        var comNota = await SemearProduto(subId, "Bem Avaliado", 10m);
        var semNota = await SemearProduto(subId, "Sem Avaliacao", 10m);
        var usuarioId = await SemearUsuario();
        await SemearAvaliacao(comNota.ProdutoId, usuarioId, nota: 5);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MelhorAvaliados);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(comNota.ProdutoId, pagina[0].ProdutoId);
        Assert.Equal(semNota.ProdutoId, pagina[1].ProdutoId);
    }

    [Fact]
    public async Task Dado_CategoriaComMaisDeUmaPagina_Quando_PercorrerTodasAsPaginas_Entao_CadaProdutoApareceUmaVez()
    {
        // CA-16: com ordenação sem empate (RN-05), Skip/Take determinístico
        // não repete nem descarta nenhum produto ao longo das páginas.
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        var nomesEsperados = new List<string>();
        for (var i = 1; i <= 25; i++)
        {
            var nome = $"Produto {i:D2}";
            nomesEsperados.Add(nome);
            await SemearProduto(subId, nome, 10m + i);
        }

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ);
        var total = await repositorio.ContarNoCatalogo(filtro);
        var totalDePaginas = (int)Math.Ceiling(total / 12.0);

        var nomesVistos = new List<string>();
        for (var pagina = 1; pagina <= totalDePaginas; pagina++)
        {
            var itens = await repositorio.BuscarPaginaDoCatalogo(filtro, pagina, 12);
            nomesVistos.AddRange(itens.Select(p => p.Nome));
        }

        Assert.Equal(25, total);
        Assert.Equal(nomesEsperados.OrderBy(n => n), nomesVistos.OrderBy(n => n));
        Assert.Equal(nomesVistos.Count, nomesVistos.Distinct().Count());
    }

    private async Task<(Guid CategoriaId, Guid SubBarrasId, Guid SubPotesId)> SemearCategoriaDoces()
    {
        var categoria = new Categoria("Doces");
        var subBarras = new Subcategoria(categoria.CategoriaId, "Barras");
        var subPotes = new Subcategoria(categoria.CategoriaId, "Potes");

        Contexto.Categorias.Add(categoria);
        Contexto.Subcategorias.AddRange(subBarras, subPotes);
        await Contexto.SaveChangesAsync();

        return (categoria.CategoriaId, subBarras.SubcategoriaId, subPotes.SubcategoriaId);
    }

    private async Task<(Guid CategoriaId, Guid SubId)> SemearCategoriaAdega()
    {
        var categoria = new Categoria("Adega");
        var subVinhos = new Subcategoria(categoria.CategoriaId, "Vinhos");

        Contexto.Categorias.Add(categoria);
        Contexto.Subcategorias.Add(subVinhos);
        await Contexto.SaveChangesAsync();

        return (categoria.CategoriaId, subVinhos.SubcategoriaId);
    }

    private async Task<Produto> SemearProduto(
        Guid subcategoriaId, string nome, decimal preco, bool semAcucar = false, ProdutoStatus status = ProdutoStatus.Ativo)
    {
        var produto = new Produto(subcategoriaId, nome, preco, "https://imagem.com/produto.jpg", status, semAcucar: semAcucar);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }
}
