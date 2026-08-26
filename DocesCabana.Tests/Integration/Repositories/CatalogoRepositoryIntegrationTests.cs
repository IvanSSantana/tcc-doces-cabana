using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.MVC.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

public class CatalogoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_ProdutoComNomeNormalizadoVazio_Quando_PreencherRetroativamente_Entao_DeveFicarEncontravel()
    {
        // Simula uma linha gravada antes desta migration: NomeNormalizado
        // vazio, exatamente o que a migration deixa nas linhas antigas
        // (spec 016, plano §6). Grava direto no contexto, sem passar pelo
        // construtor de Produto, para não preencher o campo de propósito.
        var (_, subId, _) = await SemearCategoriaDoces();
        var produto = new Produto(subId, "Café Especial", 15m, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();

        // "Apaga" o derivado via SQL cru — Produto não expõe um jeito de
        // deixá-lo divergente do nome por fora do construtor (RN-02), então
        // simular a base antiga exige contornar a entidade.
        await Contexto.Database.ExecuteSqlRawAsync(
            "UPDATE Produto SET NomeNormalizado = '' WHERE ProdutoId = {0}", produto.ProdutoId);
        Contexto.ChangeTracker.Clear();

        await DbInitializer.PreencherNomesNormalizados(Contexto);

        var produtoAtualizado = await Contexto.Produtos.AsNoTracking()
            .SingleAsync(p => p.ProdutoId == produto.ProdutoId);
        Assert.Equal("cafe especial", produtoAtualizado.NomeNormalizado);
    }

    [Fact]
    public async Task Dado_BaseComTudoPreenchido_Quando_PreencherRetroativamente_Entao_NaoDeveAlterarNada()
    {
        var (_, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Brigadeiro", 5m);

        // Idempotente: rodar sobre uma base já correta (como a recém-criada,
        // onde o construtor já preencheu tudo) não pode falhar nem duplicar
        // nada — só não encontra ninguém para corrigir.
        await DbInitializer.PreencherNomesNormalizados(Contexto);
        await DbInitializer.PreencherNomesNormalizados(Contexto);

        var produto = await Contexto.Produtos.AsNoTracking().SingleAsync();
        Assert.Equal("brigadeiro", produto.NomeNormalizado);
    }

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

    [Fact]
    public async Task Dado_ProdutoComAcento_Quando_BuscarSemAcentoEEmOutraCaixa_Entao_DeveEncontrar()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Café Especial", 10m);

        var repositorio = new ProdutoRepository(Contexto);
        // TermoNormalizado chega já normalizado do CatalogoService (RN-02)
        // — o repositório só compara, não normaliza. "cafe" é o que
        // TextoHelper.Normalizar("CAFÉ") produziria.
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, "cafe");

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Single(pagina);
        Assert.Equal("Café Especial", pagina[0].Nome);
    }

    [Fact]
    public async Task Dado_TermoNoMeioDoNome_Quando_Buscar_Entao_DeveEncontrar()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Barra de Chocolate", 10m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, "chocolate");

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Single(pagina);
    }

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_BuscarPeloNomeExato_Entao_NaoDeveEncontrar()
    {
        // RN-06: produto fora do catálogo público não existe do lado de
        // fora em nenhum caminho — inclusive na busca.
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Produto Escondido", 10m, status: ProdutoStatus.Inativo);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, "produto escondido");

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Empty(pagina);
    }

    [Fact]
    public async Task Dado_TermoComCaracteresDeCuringaSql_Quando_Buscar_Entao_NaoDeveTratarComoCuringa()
    {
        // No SQLite, Contains vira instr — literal, sem interpretar % ou _
        // como curinga (plano §9). Um produto chamado "100% Cacau" só casa
        // com o termo "100% cacau" digitado por inteiro, não com "100" mais
        // qualquer coisa.
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "100% Cacau", 10m);
        await SemearProduto(subId, "1000 Cacau", 12m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, "100% cacau");

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Single(pagina);
        Assert.Equal("100% Cacau", pagina[0].Nome);
    }

    [Fact]
    public async Task Dado_TermoNuloOuVazio_Quando_Buscar_Entao_NaoDeveFiltrarPorNome()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        await SemearProduto(subId, "Primeiro", 10m);
        await SemearProduto(subId, "Segundo", 12m);

        var repositorio = new ProdutoRepository(Contexto);
        var filtroNulo = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, null);
        var filtroVazio = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.NomeAZ, "");

        Assert.Equal(2, await repositorio.ContarNoCatalogo(filtroNulo));
        Assert.Equal(2, await repositorio.ContarNoCatalogo(filtroVazio));
    }

    // ── Ordenação por mais vendidos (spec 022) ──────────────────────────

    [Fact]
    public async Task Dado_ProdutosComVendasDiferentes_Quando_OrdenarPorMaisVendidos_Entao_OMaisVendidoDeveVirPrimeiro()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        var maisVendido = await SemearProduto(subId, "Mais Vendido", 10m);
        var poucoVendido = await SemearProduto(subId, "Pouco Vendido", 10m);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);

        await SemearPedido(usuarioId, enderecoId, PedidoStatus.Confirmado, (maisVendido.ProdutoId, 10));
        await SemearPedido(usuarioId, enderecoId, PedidoStatus.Confirmado, (poucoVendido.ProdutoId, 1));

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MaisVendidos);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(maisVendido.ProdutoId, pagina[0].ProdutoId);
        Assert.Equal(poucoVendido.ProdutoId, pagina[1].ProdutoId);
    }

    [Fact]
    public async Task Dado_ProdutoSemNenhumaVenda_Quando_OrdenarPorMaisVendidos_Entao_DeveIrParaOFimSemSumirDaConsulta()
    {
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        var vendido = await SemearProduto(subId, "Vendido", 10m);
        var semVenda = await SemearProduto(subId, "Sem Venda", 10m);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);

        await SemearPedido(usuarioId, enderecoId, PedidoStatus.Confirmado, (vendido.ProdutoId, 1));

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MaisVendidos);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(2, pagina.Count);
        Assert.Equal(vendido.ProdutoId, pagina[0].ProdutoId);
        Assert.Equal(semVenda.ProdutoId, pagina[1].ProdutoId);
    }

    [Fact]
    public async Task Dado_PedidoCancelado_Quando_OrdenarPorMaisVendidos_Entao_ANaoDeveContar()
    {
        // RN-05/CA-22: um pedido cancelado com quantidade enorme não deve
        // fazer o produto vencer um concorrente com menos vendas de verdade.
        var (categoriaId, subId, _) = await SemearCategoriaDoces();
        var vendidoDeVerdade = await SemearProduto(subId, "Vendido De Verdade", 10m);
        var soCancelado = await SemearProduto(subId, "So Cancelado", 10m);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);

        await SemearPedido(usuarioId, enderecoId, PedidoStatus.Confirmado, (vendidoDeVerdade.ProdutoId, 2));
        await SemearPedido(usuarioId, enderecoId, PedidoStatus.Cancelado, (soCancelado.ProdutoId, 1000));

        var repositorio = new ProdutoRepository(Contexto);
        var filtro = new FiltroCatalogoDTO(categoriaId, [], false, OrdenacaoCatalogo.MaisVendidos);

        var pagina = await repositorio.BuscarPaginaDoCatalogo(filtro, 1, 12);

        Assert.Equal(vendidoDeVerdade.ProdutoId, pagina[0].ProdutoId);
        Assert.Equal(soCancelado.ProdutoId, pagina[1].ProdutoId);
    }

    private async Task<Guid> SemearEndereco(Guid usuarioId)
    {
        var endereco = new Endereco(usuarioId, "SP", "Cidade Teste", "Bairro Teste", "17340001", "Rua Teste", 100);
        Contexto.Enderecos.Add(endereco);
        await Contexto.SaveChangesAsync();
        return endereco.EnderecoId;
    }

    private async Task SemearPedido(Guid usuarioId, Guid enderecoId, PedidoStatus status, params (Guid ProdutoId, short Quantidade)[] itens)
    {
        var pedido = new Pedido(usuarioId, enderecoId, 100m, 10m, "Correios", "PAC", 3, 7);
        foreach (var (produtoId, quantidade) in itens)
            pedido.AcrescentarItem(produtoId, quantidade, 10m);

        if (status == PedidoStatus.Cancelado)
            pedido.Cancelar();
        else if (status == PedidoStatus.Confirmado)
            pedido.Confirmar();

        Contexto.Pedidos.Add(pedido);
        await Contexto.SaveChangesAsync();
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
        var produto = new Produto(subcategoriaId, nome, preco, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m, status, semAcucar: semAcucar);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }
}
