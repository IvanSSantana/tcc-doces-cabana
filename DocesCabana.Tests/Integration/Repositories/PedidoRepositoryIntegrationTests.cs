using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class PedidoRepositoryIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_PedidoComItens_Quando_Buscar_Entao_DeveRetornarComOsItensEProdutoEEndereco()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);
        var subcategoriaId = await SemearSubcategoria();
        var produto = await SemearProduto(subcategoriaId, "Brigadeiro", 5m);

        var pedido = new Pedido(usuarioId, enderecoId, 20m, 10m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(produto.ProdutoId, 2, 5m);
        var pagamento = new Pagamento(pedido.PedidoId, MetodoPagamento.Pix, 20m);

        await repositorio.AdicionarComPagamento(pedido, pagamento);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, usuarioId);

        Assert.NotNull(encontrado);
        Assert.Single(encontrado.Itens);
        Assert.Equal(produto.ProdutoId, encontrado.Itens.First().ProdutoId);
        Assert.Equal(2, encontrado.Itens.First().Quantidade);
        // spec 023: o detalhe precisa do produto de cada item e do endereço,
        // sem consulta extra — os dois vêm na mesma chamada.
        Assert.Equal("Brigadeiro", encontrado.Itens.First().Produto?.Nome);
        Assert.NotNull(encontrado.EnderecoEntrega);
        Assert.Equal(enderecoId, encontrado.EnderecoEntrega!.EnderecoId);
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_Buscar_Entao_DeveRetornarNulo()
    {
        // RN-01/CA-07 (spec 023): o par pedido-e-dono é a própria barreira —
        // não existe caminho que ache o pedido só pelo identificador.
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var dono = await SemearUsuario("Dono", "52998224725");
        var outroUsuario = await SemearUsuario("Outro", "11144477735");
        var enderecoId = await SemearEndereco(dono);

        var pedido = new Pedido(dono, enderecoId, 20m, 10m, "Correios", "PAC", 3, 7);
        await repositorio.Adicionar(pedido);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, outroUsuario);

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task Dado_PedidoEPagamento_Quando_AdicionarComPagamento_Entao_DeveGravarOsDoisComUmSalvarAlteracoes()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);

        var pedido = new Pedido(usuarioId, enderecoId, 10m, 10m, "Correios", "PAC", 3, 7);
        var pagamento = new Pagamento(pedido.PedidoId, MetodoPagamento.Boleto, 10m);

        await repositorio.AdicionarComPagamento(pedido, pagamento);
        var linhasAfetadas = await unidadeDeTrabalho.SalvarAlteracoes();

        Assert.True(linhasAfetadas >= 2);

        var pagamentoGravado = await repositorio.BuscarPagamentoPorPedido(pedido.PedidoId);
        Assert.NotNull(pagamentoGravado);
        Assert.Equal(MetodoPagamento.Boleto, pagamentoGravado.Metodo);
    }

    [Fact]
    public async Task Dado_PedidosDeDoisUsuarios_Quando_ListarPorUsuario_Entao_DeveRetornarSoDoUsuarioPedido()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioA = await SemearUsuario("Cliente A", "52998224725");
        var usuarioB = await SemearUsuario("Cliente B", "11144477735");
        var enderecoA = await SemearEndereco(usuarioA);
        var enderecoB = await SemearEndereco(usuarioB);

        var pedidoDeA = new Pedido(usuarioA, enderecoA, 10m, 10m, "Correios", "PAC", 3, 7);
        var pedidoDeB = new Pedido(usuarioB, enderecoB, 15m, 10m, "Correios", "PAC", 3, 7);
        await repositorio.Adicionar(pedidoDeA);
        await repositorio.Adicionar(pedidoDeB);
        await unidadeDeTrabalho.SalvarAlteracoes();

        var pedidosDeA = await repositorio.ListarPorUsuario(usuarioA);

        Assert.Single(pedidosDeA);
        Assert.Equal(pedidoDeA.PedidoId, pedidosDeA[0].PedidoId);
    }

    private async Task<Guid> SemearEndereco(Guid usuarioId)
    {
        var endereco = new Endereco(usuarioId, "SP", "Cidade Teste", "Bairro Teste", "17340001", "Rua Teste", 100);
        endereco.MarcarComoPadrao();
        Contexto.Enderecos.Add(endereco);
        await Contexto.SaveChangesAsync();
        return endereco.EnderecoId;
    }

    private async Task<Produto> SemearProduto(Guid subcategoriaId, string nome, decimal preco)
    {
        var produto = new Produto(subcategoriaId, nome, preco, "https://imagem.com/produto.jpg", 0.5m, 10m, 15m, 20m);
        Contexto.Produtos.Add(produto);
        await Contexto.SaveChangesAsync();
        return produto;
    }
}
