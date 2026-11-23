using DocesCabana.Tests.Integration.Infraestrutura;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Domain.ValueObjects;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class PedidoRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
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
        Assert.Equal("Brigadeiro", encontrado.Itens.First().Produto?.Nome);
        Assert.NotNull(encontrado.EnderecoEntrega);
        Assert.Equal(enderecoId, encontrado.EnderecoEntrega!.EnderecoId);
    }

    [Fact]
    public async Task Dado_ItemComDesconto_Quando_Salvar_Entao_DeveVoltarComDescontoERotulo()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);
        var produto = await SemearProdutoComSubcategoria();

        var pedido = new Pedido(usuarioId, enderecoId, 20m, 10m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(produto.ProdutoId, 3, 5m, 5m, "Leve 3, pague 2");
        await repositorio.AdicionarComPagamento(pedido, new Pagamento(pedido.PedidoId, MetodoPagamento.Pix, 20m));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, usuarioId);

        var item = Assert.Single(encontrado!.Itens);
        Assert.Equal(5m, item.DescontoDaPromocao);
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
        Assert.Equal(10m, item.CalcularValorDaLinha());
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_Buscar_Entao_DeveRetornarNulo()
    {
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

    [Fact]
    public async Task Dado_PedidoEntregueComOProduto_Quando_ExisteEntregueComProduto_Entao_DeveSerVerdadeiro()
    {
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var produto = await SemearProdutoComSubcategoria();
        await SemearPedido(usuarioId, produto.ProdutoId, PedidoStatus.Entregue);

        var existe = await repositorio.ExisteEntregueComProduto(usuarioId, produto.ProdutoId);

        Assert.True(existe);
    }

    [Theory]
    [InlineData(PedidoStatus.Pendente)]
    [InlineData(PedidoStatus.Confirmado)]
    [InlineData(PedidoStatus.Enviado)]
    [InlineData(PedidoStatus.Cancelado)]
    public async Task Dado_PedidoNaoEntregueComOProduto_Quando_ExisteEntregueComProduto_Entao_DeveSerFalso(PedidoStatus status)
    {
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var produto = await SemearProdutoComSubcategoria();
        await SemearPedido(usuarioId, produto.ProdutoId, status);

        var existe = await repositorio.ExisteEntregueComProduto(usuarioId, produto.ProdutoId);

        Assert.False(existe);
    }

    [Fact]
    public async Task Dado_PedidoEntregueDeOutroUsuario_Quando_ExisteEntregueComProduto_Entao_DeveSerFalso()
    {
        var repositorio = new PedidoRepository(Contexto);
        var dono = await SemearUsuario("Dono", "52998224725");
        var outroUsuario = await SemearUsuario("Outro", "11144477735");
        var produto = await SemearProdutoComSubcategoria();
        await SemearPedido(dono, produto.ProdutoId, PedidoStatus.Entregue);

        var existe = await repositorio.ExisteEntregueComProduto(outroUsuario, produto.ProdutoId);

        Assert.False(existe);
    }

    [Fact]
    public async Task Dado_PedidoEntregueComOutroProduto_Quando_ExisteEntregueComProduto_Entao_DeveSerFalso()
    {
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var subcategoriaId = await SemearSubcategoria();
        var comprado = await SemearProduto(subcategoriaId, "Brigadeiro", 5m);
        var naoComprado = await SemearProduto(subcategoriaId, "Beijinho", 6m);
        await SemearPedido(usuarioId, comprado.ProdutoId, PedidoStatus.Entregue);

        var existe = await repositorio.ExisteEntregueComProduto(usuarioId, naoComprado.ProdutoId);

        Assert.False(existe);
    }

    private async Task<Produto> SemearProdutoComSubcategoria()
    {
        var subcategoriaId = await SemearSubcategoria();
        return await SemearProduto(subcategoriaId, "Brigadeiro", 5m);
    }

    [Fact]
    public async Task Dado_PedidoComCupom_Quando_Buscar_Entao_DeveVoltarComCodigoEDescontos()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);
        var produto = await SemearProdutoComSubcategoria();
        var cupom = await SemearCupom();

        var pedido = new Pedido(usuarioId, enderecoId, 9m, 10m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(produto.ProdutoId, 2, 5m);
        pedido.AplicarCupom(cupom.CupomId, cupom.Codigo, 1m, 10m);
        await repositorio.AdicionarComPagamento(pedido, new Pagamento(pedido.PedidoId, MetodoPagamento.Pix, 9m));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, usuarioId);

        Assert.Equal(cupom.CupomId, encontrado!.CupomId);
        Assert.Equal("BEMVINDO10", encontrado.CodigoDoCupom);
        Assert.Equal(1m, encontrado.DescontoDoCupom);
        Assert.Equal(10m, encontrado.DescontoNoFrete);
        Assert.Equal(9m, encontrado.ValorDosProdutos());
    }

    [Fact]
    public async Task Dado_PedidoPendenteComCupom_Quando_ClienteJaUsouCupom_Entao_DeveSerVerdadeiro()
    {
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var produto = await SemearProdutoComSubcategoria();
        var cupom = await SemearCupom();
        await SemearPedido(usuarioId, produto.ProdutoId, PedidoStatus.Pendente, cupom);

        Assert.True(await repositorio.ClienteJaUsouCupom(usuarioId, cupom.CupomId));
    }

    [Fact]
    public async Task Dado_PedidoCanceladoComCupom_Quando_ClienteJaUsouCupom_Entao_DeveSerFalso()
    {
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var produto = await SemearProdutoComSubcategoria();
        var cupom = await SemearCupom();
        await SemearPedido(usuarioId, produto.ProdutoId, PedidoStatus.Cancelado, cupom);

        Assert.False(await repositorio.ClienteJaUsouCupom(usuarioId, cupom.CupomId));
    }

    [Fact]
    public async Task Dado_PedidoDeOutroClienteComCupom_Quando_ClienteJaUsouCupom_Entao_DeveSerFalso()
    {
        var repositorio = new PedidoRepository(Contexto);
        var dono = await SemearUsuario("Dono", "52998224725");
        var outro = await SemearUsuario("Outro", "11144477735");
        var produto = await SemearProdutoComSubcategoria();
        var cupom = await SemearCupom();
        await SemearPedido(dono, produto.ProdutoId, PedidoStatus.Entregue, cupom);

        Assert.False(await repositorio.ClienteJaUsouCupom(outro, cupom.CupomId));
    }

    [Fact]
    public async Task Dado_PedidosComCupons_Quando_ContarUsosPorCupom_Entao_DeveIgnorarCancelados()
    {
        var repositorio = new PedidoRepository(Contexto);
        var clienteA = await SemearUsuario("Cliente A", "52998224725");
        var clienteB = await SemearUsuario("Cliente B", "11144477735");
        var produto = await SemearProdutoComSubcategoria();
        var cupom = await SemearCupom();
        await SemearPedido(clienteA, produto.ProdutoId, PedidoStatus.Pendente, cupom);
        await SemearPedido(clienteB, produto.ProdutoId, PedidoStatus.Entregue, cupom);
        await SemearPedido(clienteB, produto.ProdutoId, PedidoStatus.Cancelado, cupom);
        await SemearPedido(clienteB, produto.ProdutoId, PedidoStatus.Pendente);

        var usos = await repositorio.ContarUsosPorCupom();

        var uso = Assert.Single(usos);
        Assert.Equal(cupom.CupomId, uso.Key);
        Assert.Equal(2, uso.Value);
    }

    private async Task<Cupom> SemearCupom()
    {
        var cupom = new Cupom("BEMVINDO10", TipoDeDesconto.Percentual, 10, null, null,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, true, null, null);
        Contexto.Cupons.Add(cupom);
        await Contexto.SaveChangesAsync();
        return cupom;
    }

    private async Task SemearPedido(Guid usuarioId, Guid produtoId, PedidoStatus status, Cupom? cupom = null)
    {
        var enderecoId = await SemearEndereco(usuarioId);
        var pedido = new Pedido(usuarioId, enderecoId, 20m, 10m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(produtoId, 1, 5m);

        if (cupom is not null)
            pedido.AplicarCupom(cupom.CupomId, cupom.Codigo, 0.5m, 0m);

        switch (status)
        {
            case PedidoStatus.Confirmado: pedido.Confirmar(); break;
            case PedidoStatus.Enviado: pedido.Confirmar(); pedido.MarcarComoEnviado(); break;
            case PedidoStatus.Entregue: pedido.Confirmar(); pedido.MarcarComoEnviado(); pedido.MarcarComoEntregue(); break;
            case PedidoStatus.Cancelado: pedido.Cancelar(); break;
        }

        Contexto.Pedidos.Add(pedido);
        await Contexto.SaveChangesAsync();
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

    [Fact]
    public async Task Dado_PedidoDeRetirada_Quando_Salvar_Entao_DevePersistirSemEndereco()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var produto = await SemearProdutoComSubcategoria();
        var faixa = new FaixaDeRetirada(new DateOnly(2026, 11, 23), new TimeOnly(14, 0), new TimeOnly(15, 0));

        var pedido = Pedido.ParaRetirada(usuarioId, 10m, faixa);
        pedido.AcrescentarItem(produto.ProdutoId, 2, 5m);
        await repositorio.AdicionarComPagamento(pedido, new Pagamento(pedido.PedidoId, MetodoPagamento.Pix, 10m));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, usuarioId);

        Assert.NotNull(encontrado);
        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, encontrado.Modalidade);
        Assert.Null(encontrado.EnderecoEntregaId);
        Assert.Null(encontrado.EnderecoEntrega);
        Assert.Null(encontrado.Transportadora);
        Assert.Null(encontrado.PrazoMinimoEmDias);
        Assert.Equal(0m, encontrado.ValorDoFrete);
        Assert.Equal(new DateOnly(2026, 11, 23), encontrado.DataDaRetirada);
        Assert.Equal(new TimeOnly(14, 0), encontrado.InicioDaRetirada);
        Assert.Equal(new TimeOnly(15, 0), encontrado.FimDaRetirada);
        Assert.Single(encontrado.Itens);
    }

    [Fact]
    public async Task Dado_PedidoDeEntrega_Quando_Salvar_Entao_DeveVoltarComModalidadeEntrega()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new PedidoRepository(Contexto);
        var usuarioId = await SemearUsuario();
        var enderecoId = await SemearEndereco(usuarioId);

        var pedido = new Pedido(usuarioId, enderecoId, 20m, 10m, "Correios", "PAC", 3, 7);
        await repositorio.AdicionarComPagamento(pedido, new Pagamento(pedido.PedidoId, MetodoPagamento.Pix, 20m));
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.Buscar(pedido.PedidoId, usuarioId);

        Assert.Equal(ModalidadeDeEntrega.Entrega, encontrado!.Modalidade);
        Assert.Null(encontrado.DataDaRetirada);
        Assert.Equal(3, encontrado.PrazoMinimoEmDias);
    }
}
