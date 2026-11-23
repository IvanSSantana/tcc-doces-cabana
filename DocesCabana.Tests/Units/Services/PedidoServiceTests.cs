using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class PedidoServiceTests
{
    private readonly Mock<ICarrinhoService> _carrinhoServiceMock;
    private readonly Mock<IItemCarrinhoRepository> _itemCarrinhoRepositoryMock;
    private readonly Mock<IEnderecoService> _enderecoServiceMock;
    private readonly Mock<IFreteService> _freteServiceMock;
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICupomService> _cupomServiceMock;
    private readonly Mock<IAgendaDeRetiradaService> _agendaDeRetiradaServiceMock;
    private readonly PedidoService _pedidoService;

    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _enderecoId = Guid.NewGuid();
    private readonly Guid _produtoId = Guid.NewGuid();
    private const int _servicoDeEntregaId = 1;

    public PedidoServiceTests()
    {
        _carrinhoServiceMock = new Mock<ICarrinhoService>();
        _itemCarrinhoRepositoryMock = new Mock<IItemCarrinhoRepository>();
        _enderecoServiceMock = new Mock<IEnderecoService>();
        _freteServiceMock = new Mock<IFreteService>();
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cupomServiceMock = new Mock<ICupomService>();
        _agendaDeRetiradaServiceMock = new Mock<IAgendaDeRetiradaService>();

        _pedidoService = new PedidoService(
            _carrinhoServiceMock.Object,
            _itemCarrinhoRepositoryMock.Object,
            _enderecoServiceMock.Object,
            _freteServiceMock.Object,
            _pedidoRepositoryMock.Object,
            _cupomServiceMock.Object,
            _agendaDeRetiradaServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private LinhaDoCarrinhoDTO CriarLinha(
        decimal preco = 10m, short quantidade = 1, MotivoIndisponibilidade motivo = MotivoIndisponibilidade.Nenhum) =>
        new()
        {
            ProdutoId = _produtoId,
            Nome = "Brigadeiro",
            ImagemUrl = "https://imagem.com/brigadeiro.jpg",
            PrecoUnitario = preco,
            Quantidade = quantidade,
            ValorDaLinha = preco * quantidade,
            MotivoIndisponibilidade = motivo,
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
        };

    private static CarrinhoDTO CriarCarrinho(params LinhaDoCarrinhoDTO[] linhas) => new()
    {
        Linhas = linhas.ToList(),
        Subtotal = linhas.Where(l => l.Disponivel).Sum(l => l.ValorDaLinha),
        TotalDeItens = linhas.Sum(l => (int)l.Quantidade)
    };

    private FechamentoDePedidoDTO CriarDados(
        decimal valorDosProdutosExibido = 10m, decimal valorDoFreteExibido = 8m, int servicoDeEntregaId = _servicoDeEntregaId) =>
        new()
        {
            EnderecoId = _enderecoId,
            ServicoDeEntregaId = servicoDeEntregaId,
            MetodoPagamento = MetodoPagamento.Pix,
            ValorDosProdutosExibido = valorDosProdutosExibido,
            ValorDoFreteExibido = valorDoFreteExibido
        };

    private void PrepararEnderecoValido() =>
        _enderecoServiceMock
            .Setup(s => s.BuscarDoUsuario(_enderecoId, _usuarioId))
            .ReturnsAsync(new EnderecoDTO { EnderecoId = _enderecoId, CEP = "17340001", Estado = "SP", Cidade = "Cidade", Bairro = "Bairro", Rua = "Rua", Numero = 1 });

    private void PrepararCotacaoValida(decimal preco = 8m, int prazoMin = 3, int prazoMax = 7) =>
        _freteServiceMock
            .Setup(s => s.Cotar("17340001", It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO(
                "17340001",
                [new OpcaoDeFreteDTO(_servicoDeEntregaId, "Correios", "PAC", preco, prazoMin, prazoMax)],
                null));

    [Fact]
    public async Task Dado_CarrinhoVazio_Quando_Fechar_Entao_DeveRecusarSemLancar()
    {
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados());

        Assert.False(resultado.Sucesso);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ItemIndisponivel_Quando_Fechar_Entao_DeveRecusarNomeandoOItem()
    {
        var linha = CriarLinha(motivo: MotivoIndisponibilidade.ForaDeEstoque);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Brigadeiro", resultado.ItemIndisponivel);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ValorDosProdutosDivergente_Quando_Fechar_Entao_DeveRecusarDevolvendoOAtual()
    {
        var linha = CriarLinha(preco: 12m);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 10m));

        Assert.False(resultado.Sucesso);
        Assert.Equal(12m, resultado.ValorDosProdutosAtual);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ValoresDeCentavo_Quando_Fechar_Entao_ArredondamentoNaoDeveRecusarFechamentoLegitimo()
    {
        var linha = CriarLinha(preco: 9.99m, quantidade: 3);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        PrepararCotacaoValida();
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 29.97m));

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task Dado_CotacaoIndisponivel_Quando_Fechar_Entao_DeveRecusar()
    {
        var linha = CriarLinha();
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        _freteServiceMock
            .Setup(s => s.Cotar("17340001", It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO("17340001", [], "Não foi possível calcular o frete agora."));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Não foi possível calcular o frete agora.", resultado.Mensagem);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_OpcaoDeEntregaEscolhidaSumiuDaRecotacao_Quando_Fechar_Entao_DeveRecusar()
    {
        var linha = CriarLinha();
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        _freteServiceMock
            .Setup(s => s.Cotar("17340001", It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO("17340001", [new OpcaoDeFreteDTO(999, "Correios", "SEDEX", 20m, 1, 2)], null));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(servicoDeEntregaId: _servicoDeEntregaId));

        Assert.False(resultado.Sucesso);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ValorDoFreteDivergente_Quando_Fechar_Entao_DeveRecusarDevolvendoOAtual()
    {
        var linha = CriarLinha();
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        PrepararCotacaoValida(preco: 15m);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDoFreteExibido: 8m));

        Assert.False(resultado.Sucesso);
        Assert.Equal(15m, resultado.ValorDoFreteAtual);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_TudoValido_Quando_Fechar_Entao_DeveGravarPedidoItensEPagamentoEEsvaziarComUmSalvarAlteracoes()
    {
        var linha = CriarLinha(preco: 10m, quantidade: 2);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        PrepararCotacaoValida(preco: 8m, prazoMin: 3, prazoMax: 7);

        var itemDoCarrinho = new ItemCarrinho(_usuarioId, _produtoId, 2);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([itemDoCarrinho]);

        Pedido? pedidoGravado = null;
        Pagamento? pagamentoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, pag) => { pedidoGravado = p; pagamentoGravado = pag; })
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 20m, valorDoFreteExibido: 8m));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(pedidoGravado);
        Assert.Single(pedidoGravado!.Itens);
        Assert.Equal(10m, pedidoGravado.Itens.First().PrecoUnitario);
        Assert.Equal(28m, pedidoGravado.Valor);
        Assert.Equal(8m, pedidoGravado.ValorDoFrete);
        Assert.NotNull(pagamentoGravado);
        Assert.Equal(MetodoPagamento.Pix, pagamentoGravado!.Metodo);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(itemDoCarrinho), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_PrecoDoProdutoMudouAposRevisao_Quando_FecharComValorDeAgora_Entao_DevePersistirOPrecoCongelado()
    {
        var linha = CriarLinha(preco: 11m, quantidade: 1);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        PrepararCotacaoValida(preco: 8m);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);

        Pedido? pedidoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => pedidoGravado = p)
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 11m, valorDoFreteExibido: 8m));

        Assert.True(resultado.Sucesso);
        Assert.Equal(11m, pedidoGravado!.Itens.First().PrecoUnitario);
    }

    private LinhaDoCarrinhoDTO CriarLinhaComLeve3Pague2() => new()
    {
        ProdutoId = _produtoId,
        Nome = "Brigadeiro",
        ImagemUrl = "https://imagem.com/brigadeiro.jpg",
        PrecoUnitario = 10m,
        Quantidade = 3,
        DescontoDaPromocao = 10m,
        RotuloDaPromocao = "Leve 3, pague 2",
        ValorDaLinha = 20m,
        Peso = 0.5m,
        Altura = 10m,
        Largura = 15m,
        Comprimento = 20m
    };

    private void PrepararFechamentoComPromocao()
    {
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(CriarLinhaComLeve3Pague2()));
        PrepararEnderecoValido();
        PrepararCotacaoValida(preco: 8m);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);
    }

    [Fact]
    public async Task Dado_CarrinhoComPromocao_Quando_Fechar_Entao_ItemDeveTerDescontoERotulo()
    {
        PrepararFechamentoComPromocao();
        Pedido? pedidoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => pedidoGravado = p)
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 20m, valorDoFreteExibido: 8m));

        Assert.True(resultado.Sucesso);
        var item = Assert.Single(pedidoGravado!.Itens);
        Assert.Equal(10m, item.PrecoUnitario);
        Assert.Equal(10m, item.DescontoDaPromocao);
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
    }

    [Fact]
    public async Task Dado_CarrinhoComPromocao_Quando_Fechar_Entao_ValorDoPedidoDeveSerLiquidoMaisFrete()
    {
        PrepararFechamentoComPromocao();
        Pedido? pedidoGravado = null;
        Pagamento? pagamentoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, pag) => { pedidoGravado = p; pagamentoGravado = pag; })
            .Returns(Task.CompletedTask);

        await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 20m, valorDoFreteExibido: 8m));

        Assert.Equal(28m, pedidoGravado!.Valor);
        Assert.Equal(28m, pagamentoGravado!.Valor);
    }

    [Fact]
    public async Task Dado_SubtotalExibidoSemPromocaoQueComecou_Quando_Fechar_Entao_DeveRecusarComValorAtual()
    {
        PrepararFechamentoComPromocao();

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 30m, valorDoFreteExibido: 8m));

        Assert.False(resultado.Sucesso);
        Assert.Equal(20m, resultado.ValorDosProdutosAtual);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    private Pedido CriarPedidoPersistido(DateTime data, PedidoStatus status = PedidoStatus.Pendente)
    {
        var pedido = new Pedido(_usuarioId, _enderecoId, 28m, 8m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(_produtoId, 2, 10m);
        DefinirData(pedido, data);
        if (status == PedidoStatus.Confirmado) pedido.Confirmar();
        return pedido;
    }

    private static void DefinirData(Pedido pedido, DateTime data) =>
        typeof(Pedido).GetProperty(nameof(Pedido.Data))!.SetValue(pedido, data);

    [Fact]
    public async Task Dado_PedidosDoUsuario_Quando_ListarDoUsuario_Entao_DeveDevolverDoMaisRecenteAoMaisAntigo()
    {
        var antigo = CriarPedidoPersistido(new DateTime(2026, 1, 1));
        var recente = CriarPedidoPersistido(new DateTime(2026, 6, 1));
        _pedidoRepositoryMock.Setup(r => r.ListarPorUsuario(_usuarioId)).ReturnsAsync([antigo, recente]);

        var resultado = await _pedidoService.ListarDoUsuario(_usuarioId);

        Assert.Equal([recente.PedidoId, antigo.PedidoId], resultado.Select(r => r.PedidoId));
    }

    [Fact]
    public async Task Dado_UsuarioSemPedido_Quando_ListarDoUsuario_Entao_DeveDevolverListaVazia()
    {
        _pedidoRepositoryMock.Setup(r => r.ListarPorUsuario(_usuarioId)).ReturnsAsync([]);

        var resultado = await _pedidoService.ListarDoUsuario(_usuarioId);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException()
    {
        var pedidoId = Guid.NewGuid();
        _pedidoRepositoryMock.Setup(r => r.Buscar(pedidoId, _usuarioId)).ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _pedidoService.BuscarDetalhe(pedidoId, _usuarioId));
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException()
    {
        var pedidoId = Guid.NewGuid();
        _pedidoRepositoryMock.Setup(r => r.Buscar(pedidoId, _usuarioId)).ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _pedidoService.BuscarDetalhe(pedidoId, _usuarioId));
    }

    [Fact]
    public async Task Dado_PrecoDoProdutoMudouDepoisDaCompra_Quando_BuscarDetalhe_Entao_DeveTrazerOPrecoGravado()
    {
        var pedido = CriarPedidoPersistido(new DateTime(2026, 1, 1));
        _pedidoRepositoryMock.Setup(r => r.Buscar(pedido.PedidoId, _usuarioId)).ReturnsAsync(pedido);
        _pedidoRepositoryMock.Setup(r => r.BuscarPagamentoPorPedido(pedido.PedidoId)).ReturnsAsync((Pagamento?)null);

        var detalhe = await _pedidoService.BuscarDetalhe(pedido.PedidoId, _usuarioId);

        Assert.Equal(10m, detalhe.Itens.First().PrecoUnitario);
    }

    // Cupom (035)

    private readonly Guid _cupomId = Guid.NewGuid();

    private ResultadoDoCupomDTO CupomValido(decimal nosProdutos = 2m, decimal noFrete = 0m) => new()
    {
        Codigo = "BEMVINDO10",
        CupomId = _cupomId,
        Valido = true,
        Rotulo = "10%",
        DescontoNosProdutos = nosProdutos,
        DescontoNoFrete = noFrete
    };

    private void PrepararFechamentoComCupom(ResultadoDoCupomDTO resultado)
    {
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(CriarLinha(10m, 2)));
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);
        PrepararEnderecoValido();
        PrepararCotacaoValida(preco: 8m);
        _cupomServiceMock
            .Setup(s => s.Avaliar("BEMVINDO10", _usuarioId, It.IsAny<CarrinhoDTO>(), 8m))
            .ReturnsAsync(resultado);
    }

    private FechamentoDePedidoDTO CriarDadosComCupom(decimal descontoExibido)
    {
        var dados = CriarDados(valorDosProdutosExibido: 20m, valorDoFreteExibido: 8m);
        dados.CodigoDoCupom = "BEMVINDO10";
        dados.DescontoDoCupomExibido = descontoExibido;
        return dados;
    }

    [Fact]
    public async Task Dado_CupomValido_Quando_Fechar_Entao_PedidoDeveTerCupomDescontosEValorLiquido()
    {
        PrepararFechamentoComCupom(CupomValido(nosProdutos: 2m));
        Pedido? pedidoGravado = null;
        Pagamento? pagamentoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, pag) => { pedidoGravado = p; pagamentoGravado = pag; })
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosComCupom(2m));

        Assert.True(resultado.Sucesso);
        Assert.Equal(_cupomId, pedidoGravado!.CupomId);
        Assert.Equal("BEMVINDO10", pedidoGravado.CodigoDoCupom);
        Assert.Equal(2m, pedidoGravado.DescontoDoCupom);
        Assert.Equal(0m, pedidoGravado.DescontoNoFrete);
        Assert.Equal(18m, pedidoGravado.ValorDosProdutos());
        Assert.Equal(26m, pedidoGravado.Valor);
        Assert.Equal(26m, pagamentoGravado!.Valor);
    }

    [Fact]
    public async Task Dado_CupomDeFreteGratis_Quando_Fechar_Entao_PedidoDeveDescontarOFrete()
    {
        PrepararFechamentoComCupom(CupomValido(nosProdutos: 0m, noFrete: 8m));
        Pedido? pedidoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => pedidoGravado = p)
            .Returns(Task.CompletedTask);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosComCupom(8m));

        Assert.True(resultado.Sucesso);
        Assert.Equal(8m, pedidoGravado!.ValorDoFrete);
        Assert.Equal(8m, pedidoGravado.DescontoNoFrete);
        Assert.Equal(20m, pedidoGravado.Valor);
    }

    [Fact]
    public async Task Dado_CupomExpiradoNoFechamento_Quando_Fechar_Entao_DeveRecusarComCupomRecusado()
    {
        PrepararFechamentoComCupom(ResultadoDoCupomDTO.ParaRecusa("BEMVINDO10", "Este cupom expirou."));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosComCupom(2m));

        Assert.False(resultado.Sucesso);
        Assert.True(resultado.CupomRecusado);
        Assert.Equal("Este cupom expirou.", resultado.Mensagem);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DescontoExibidoDiferente_Quando_Fechar_Entao_DeveRecusarComDescontoAtual()
    {
        PrepararFechamentoComCupom(CupomValido(nosProdutos: 2m));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosComCupom(3m));

        Assert.False(resultado.Sucesso);
        Assert.False(resultado.CupomRecusado);
        Assert.Equal("O desconto do cupom mudou desde a última vez que você revisou. Confira o valor atual.", resultado.Mensagem);
        Assert.Equal(2m, resultado.DescontoDoCupomAtual);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CupomQueZeraOPedido_Quando_Fechar_Entao_DeveRecusarSemCriarPagamento()
    {
        PrepararFechamentoComCupom(CupomValido(nosProdutos: 20m, noFrete: 8m));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosComCupom(28m));

        Assert.False(resultado.Sucesso);
        Assert.Equal("Não foi possível aplicar este cupom a um pedido sem valor.", resultado.Mensagem);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_SemCodigoDeCupom_Quando_Fechar_Entao_NaoDeveAvaliarCupom()
    {
        PrepararFechamentoComCupom(CupomValido());

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 20m, valorDoFreteExibido: 8m));

        Assert.True(resultado.Sucesso);
        _cupomServiceMock.Verify(s => s.Avaliar(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CarrinhoDTO>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PassoCarrinhoComFreteGratis_Quando_MontarPasso_Entao_DeveUsarCotacaoMaisBarata()
    {
        var carrinho = CriarCarrinho(CriarLinha(10m, 2)).ComCotacao(new CotacaoDeFreteDTO("17340001",
            [
                new OpcaoDeFreteDTO(1, "Correios", "SEDEX", 15m, 1, 2),
                new OpcaoDeFreteDTO(2, "Correios", "PAC", 9m, 3, 7)
            ], null));
        _cupomServiceMock
            .Setup(s => s.Avaliar("BEMVINDO10", _usuarioId, carrinho, 9m))
            .ReturnsAsync(CupomValido(nosProdutos: 0m, noFrete: 9m));

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Carrinho, carrinho, _usuarioId, null, codigoDoCupom: "BEMVINDO10");

        Assert.NotNull(passo.Cupom);
        Assert.Equal(9m, passo.DescontoTotalDoCupom);
        Assert.Equal(9m, passo.ValorDoFreteDeReferencia);
        Assert.Equal(20m, passo.ValorTotal);
    }

    [Fact]
    public async Task Dado_PassoPagamentoComFreteGratis_Quando_MontarPasso_Entao_DeveUsarOpcaoEscolhida()
    {
        var carrinho = CriarCarrinho(CriarLinha(10m, 2));
        _enderecoServiceMock.Setup(s => s.ListarDoUsuario(_usuarioId)).ReturnsAsync(
            [new EnderecoDTO { EnderecoId = _enderecoId, CEP = "17340001", Padrao = true }]);
        _freteServiceMock
            .Setup(s => s.Cotar("17340001", It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO("17340001",
            [
                new OpcaoDeFreteDTO(1, "Correios", "SEDEX", 15m, 1, 2),
                new OpcaoDeFreteDTO(2, "Correios", "PAC", 9m, 3, 7)
            ], null));
        _cupomServiceMock
            .Setup(s => s.Avaliar("BEMVINDO10", _usuarioId, carrinho, 15m))
            .ReturnsAsync(CupomValido(nosProdutos: 0m, noFrete: 15m));

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Pagamento, carrinho, _usuarioId, _enderecoId, servicoDeEntregaId: 1, codigoDoCupom: "BEMVINDO10");

        Assert.Equal(15m, passo.DescontoTotalDoCupom);
        Assert.Equal(15m, passo.ValorDoFreteDeReferencia);
        Assert.Equal(20m, passo.ValorTotal);
    }

    [Fact]
    public async Task Dado_CupomQueDeixouDeValer_Quando_MontarPasso_Entao_DeveTrazerMotivoSemDesconto()
    {
        var carrinho = CriarCarrinho(CriarLinha(10m, 2));
        _cupomServiceMock
            .Setup(s => s.Avaliar("BEMVINDO10", _usuarioId, carrinho, null))
            .ReturnsAsync(ResultadoDoCupomDTO.ParaRecusa("BEMVINDO10", "Este cupom expirou."));

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Carrinho, carrinho, _usuarioId, null, codigoDoCupom: "BEMVINDO10");

        Assert.Equal("Este cupom expirou.", passo.Cupom!.Motivo);
        Assert.Equal(0m, passo.DescontoTotalDoCupom);
        Assert.Equal(20m, passo.ValorTotal);
    }

    [Fact]
    public async Task Dado_VisitanteComCodigo_Quando_MontarPasso_Entao_NaoDeveAvaliarCupom()
    {
        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Carrinho, CriarCarrinho(CriarLinha()), null, null, codigoDoCupom: "BEMVINDO10");

        Assert.Null(passo.Cupom);
        _cupomServiceMock.Verify(s => s.Avaliar(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CarrinhoDTO>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_PedidoComCupom_Quando_BuscarDetalhe_Entao_DeveTrazerCupomEDescontos()
    {
        var pedido = new Pedido(_usuarioId, _enderecoId, 18m, 8m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(_produtoId, 2, 10m);
        pedido.AplicarCupom(_cupomId, "FRETEGRATIS", 2m, 8m);
        _pedidoRepositoryMock.Setup(r => r.Buscar(pedido.PedidoId, _usuarioId)).ReturnsAsync(pedido);

        var detalhe = await _pedidoService.BuscarDetalhe(pedido.PedidoId, _usuarioId);

        Assert.Equal("FRETEGRATIS", detalhe.CodigoDoCupom);
        Assert.Equal(2m, detalhe.DescontoDoCupom);
        Assert.Equal(8m, detalhe.DescontoNoFrete);
        Assert.Equal(18m, detalhe.ValorDosProdutos);
        Assert.Equal(20m, detalhe.ValorDosItens);
        Assert.Equal(18m, detalhe.ValorTotal);
    }

    [Fact]
    public async Task Dado_PedidoComCupom_Quando_ObterConfirmacao_Entao_DeveTrazerCupomEDescontos()
    {
        var pedido = new Pedido(_usuarioId, _enderecoId, 26m, 8m, "Correios", "PAC", 3, 7);
        pedido.AcrescentarItem(_produtoId, 2, 10m);
        pedido.AplicarCupom(_cupomId, "BEMVINDO10", 2m, 0m);
        _pedidoRepositoryMock.Setup(r => r.Buscar(pedido.PedidoId, _usuarioId)).ReturnsAsync(pedido);

        var confirmacao = await _pedidoService.ObterConfirmacao(pedido.PedidoId, _usuarioId);

        Assert.Equal("BEMVINDO10", confirmacao!.CodigoDoCupom);
        Assert.Equal(2m, confirmacao.DescontoDoCupom);
        Assert.Equal(18m, confirmacao.ValorDosProdutos);
        Assert.Equal(26m, confirmacao.ValorTotal);
    }

    // Retirada na loja (036)

    private static readonly DateOnly _hoje = new(2026, 11, 23);

    private void PrepararRetiradaDisponivel(params (int Inicio, int Fim)[] faixas) =>
        _agendaDeRetiradaServiceMock.Setup(s => s.ConsultarHoje()).ReturnsAsync(new RetiradaDisponivelDTO
        {
            Data = _hoje,
            Faixas = faixas.Select(f => new FaixaDeRetiradaDTO(new TimeOnly(f.Inicio, 0), new TimeOnly(f.Fim, 0))).ToList()
        });

    private FechamentoDePedidoDTO CriarDadosDeRetirada(int inicioDaFaixa = 14, decimal valorDosProdutosExibido = 10m) => new()
    {
        Modalidade = ModalidadeDeEntrega.RetiradaNaLoja,
        InicioDaFaixa = new TimeOnly(inicioDaFaixa, 0),
        MetodoPagamento = MetodoPagamento.Pix,
        ValorDosProdutosExibido = valorDosProdutosExibido
    };

    private void PrepararCarrinhoDeRetirada()
    {
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(CriarLinha()));
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);
    }

    [Fact]
    public async Task Dado_PassoEnderecoComRetirada_Quando_MontarPasso_Entao_DeveTrazerFaixasSemCotarFrete()
    {
        PrepararRetiradaDisponivel((14, 15), (15, 16));
        _enderecoServiceMock.Setup(s => s.ListarDoUsuario(_usuarioId)).ReturnsAsync(
            [new EnderecoDTO { EnderecoId = _enderecoId, CEP = "17340001", Padrao = true }]);

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Endereco, CriarCarrinho(CriarLinha()), _usuarioId, null,
            modalidade: ModalidadeDeEntrega.RetiradaNaLoja);

        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, passo.Modalidade);
        Assert.NotNull(passo.Retirada);
        Assert.Equal(2, passo.Retirada!.Faixas.Count);
        Assert.Equal(new TimeOnly(14, 0), passo.InicioDaFaixaSelecionada);
        Assert.Null(passo.Cotacao);
        Assert.Equal(0m, passo.ValorDoFreteDeReferencia);
        Assert.Equal(10m, passo.ValorTotal);
        _freteServiceMock.Verify(s => s.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Never);
    }

    [Fact]
    public async Task Dado_FaixaEscolhidaDisponivel_Quando_MontarPasso_Entao_DeveManterAFaixaEscolhida()
    {
        PrepararRetiradaDisponivel((14, 15), (15, 16));

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Pagamento, CriarCarrinho(CriarLinha()), _usuarioId, null,
            modalidade: ModalidadeDeEntrega.RetiradaNaLoja, inicioDaFaixa: new TimeOnly(15, 0));

        Assert.Equal(new TimeOnly(15, 0), passo.InicioDaFaixaSelecionada);
    }

    [Fact]
    public async Task Dado_RetiradaIndisponivel_Quando_MontarPasso_Entao_DeveVoltarParaEntregaSemFaixaSelecionada()
    {
        PrepararRetiradaDisponivel();
        _enderecoServiceMock.Setup(s => s.ListarDoUsuario(_usuarioId)).ReturnsAsync([]);

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Endereco, CriarCarrinho(CriarLinha()), _usuarioId, null,
            modalidade: ModalidadeDeEntrega.RetiradaNaLoja);

        Assert.False(passo.Retirada!.Disponivel);
        Assert.Null(passo.InicioDaFaixaSelecionada);
        Assert.Equal(ModalidadeDeEntrega.Entrega, passo.Modalidade);
        Assert.Null(passo.ValorDoFreteDeReferencia);
    }

    [Fact]
    public async Task Dado_PassoEnderecoComEntrega_Quando_MontarPasso_Entao_DeveTrazerDisponibilidadeDaRetirada()
    {
        PrepararRetiradaDisponivel((14, 15));
        _enderecoServiceMock.Setup(s => s.ListarDoUsuario(_usuarioId)).ReturnsAsync([]);

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Endereco, CriarCarrinho(CriarLinha()), _usuarioId, null);

        Assert.Equal(ModalidadeDeEntrega.Entrega, passo.Modalidade);
        Assert.True(passo.Retirada!.Disponivel);
        Assert.Null(passo.InicioDaFaixaSelecionada);
    }

    [Fact]
    public async Task Dado_RetiradaComCupom_Quando_MontarPasso_Entao_DeveAvaliarCupomComModalidadeRetirada()
    {
        PrepararRetiradaDisponivel((14, 15));
        var carrinho = CriarCarrinho(CriarLinha());
        _cupomServiceMock
            .Setup(s => s.Avaliar("FRETEGRATIS", _usuarioId, carrinho, 0m, ModalidadeDeEntrega.RetiradaNaLoja))
            .ReturnsAsync(ResultadoDoCupomDTO.ParaRecusa("FRETEGRATIS", "Cupom de frete grátis não vale para retirada na loja."));

        var passo = await _pedidoService.MontarPasso(
            PassoDoFechamento.Endereco, carrinho, _usuarioId, null, codigoDoCupom: "FRETEGRATIS",
            modalidade: ModalidadeDeEntrega.RetiradaNaLoja);

        Assert.Equal("Cupom de frete grátis não vale para retirada na loja.", passo.Cupom!.Motivo);
    }

    [Fact]
    public async Task Dado_Retirada_Quando_Fechar_Entao_DeveGravarPedidoDeRetiradaSemCotarFrete()
    {
        PrepararCarrinhoDeRetirada();
        PrepararRetiradaDisponivel((14, 15), (15, 16));
        Pedido? pedidoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => pedidoGravado = p);

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosDeRetirada(inicioDaFaixa: 15));

        Assert.True(resultado.Sucesso);
        Assert.Equal(ModalidadeDeEntrega.RetiradaNaLoja, pedidoGravado!.Modalidade);
        Assert.Equal(_hoje, pedidoGravado.DataDaRetirada);
        Assert.Equal(new TimeOnly(15, 0), pedidoGravado.InicioDaRetirada);
        Assert.Equal(new TimeOnly(16, 0), pedidoGravado.FimDaRetirada);
        Assert.Equal(0m, pedidoGravado.ValorDoFrete);
        Assert.Equal(10m, pedidoGravado.Valor);
        Assert.Null(pedidoGravado.EnderecoEntregaId);
        _freteServiceMock.Verify(s => s.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Never);
        _enderecoServiceMock.Verify(s => s.BuscarDoUsuario(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_FaixaQueJaPassou_Quando_Fechar_Entao_DeveRecusarComMensagemDeFaixa()
    {
        PrepararCarrinhoDeRetirada();
        PrepararRetiradaDisponivel((15, 16));

        var resultado = await _pedidoService.Fechar(_usuarioId, CriarDadosDeRetirada(inicioDaFaixa: 14));

        Assert.False(resultado.Sucesso);
        Assert.Equal("A faixa de retirada escolhida não está mais disponível. Escolha outra.", resultado.Mensagem);
        _pedidoRepositoryMock.Verify(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CupomFreteGratisNaRetirada_Quando_Fechar_Entao_DeveRecusarComCupomRecusado()
    {
        PrepararCarrinhoDeRetirada();
        PrepararRetiradaDisponivel((14, 15));
        _cupomServiceMock
            .Setup(s => s.Avaliar("FRETEGRATIS", _usuarioId, It.IsAny<CarrinhoDTO>(), 0m, ModalidadeDeEntrega.RetiradaNaLoja))
            .ReturnsAsync(ResultadoDoCupomDTO.ParaRecusa("FRETEGRATIS", "Cupom de frete grátis não vale para retirada na loja."));
        var dados = CriarDadosDeRetirada();
        dados.CodigoDoCupom = "FRETEGRATIS";

        var resultado = await _pedidoService.Fechar(_usuarioId, dados);

        Assert.False(resultado.Sucesso);
        Assert.True(resultado.CupomRecusado);
        Assert.Equal("Cupom de frete grátis não vale para retirada na loja.", resultado.Mensagem);
    }

    [Fact]
    public async Task Dado_RetiradaComCupomPercentual_Quando_Fechar_Entao_DeveDescontarSoOsProdutos()
    {
        PrepararCarrinhoDeRetirada();
        PrepararRetiradaDisponivel((14, 15));
        _cupomServiceMock
            .Setup(s => s.Avaliar("BEMVINDO10", _usuarioId, It.IsAny<CarrinhoDTO>(), 0m, ModalidadeDeEntrega.RetiradaNaLoja))
            .ReturnsAsync(CupomValido(nosProdutos: 1m));
        Pedido? pedidoGravado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => pedidoGravado = p);
        var dados = CriarDadosDeRetirada();
        dados.CodigoDoCupom = "BEMVINDO10";
        dados.DescontoDoCupomExibido = 1m;

        var resultado = await _pedidoService.Fechar(_usuarioId, dados);

        Assert.True(resultado.Sucesso);
        Assert.Equal(9m, pedidoGravado!.Valor);
        Assert.Equal(1m, pedidoGravado.DescontoDoCupom);
    }

    [Fact]
    public async Task Dado_CarrinhoValido_Quando_Fechar_Entao_PedidoDeveNascerConfirmado()
    {
        var linha = CriarLinha(preco: 9.99m, quantidade: 3);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(_usuarioId)).ReturnsAsync(CriarCarrinho(linha));
        PrepararEnderecoValido();
        PrepararCotacaoValida();
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(_usuarioId)).ReturnsAsync([]);
        Pedido? criado = null;
        _pedidoRepositoryMock
            .Setup(r => r.AdicionarComPagamento(It.IsAny<Pedido>(), It.IsAny<Pagamento>()))
            .Callback<Pedido, Pagamento>((p, _) => criado = p)
            .Returns(Task.CompletedTask);

        await _pedidoService.Fechar(_usuarioId, CriarDados(valorDosProdutosExibido: 29.97m));

        Assert.Equal(PedidoStatus.Confirmado, criado!.Status);
    }

    [Fact]
    public async Task Dado_PedidoConfirmadoDoUsuario_Quando_MarcarComoEntregue_Entao_DeveFicarEntregueESalvarAlteracoes()
    {
        var pedido = CriarPedidoPersistido(DateTime.UtcNow, PedidoStatus.Confirmado);
        _pedidoRepositoryMock.Setup(r => r.BuscarParaAlterar(pedido.PedidoId, _usuarioId)).ReturnsAsync(pedido);

        await _pedidoService.MarcarComoEntregue(pedido.PedidoId, _usuarioId);

        Assert.Equal(PedidoStatus.Entregue, pedido.Status);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Once);
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_MarcarComoEntregue_Entao_DeveLancarKeyNotFoundException()
    {
        _pedidoRepositoryMock
            .Setup(r => r.BuscarParaAlterar(It.IsAny<Guid>(), _usuarioId))
            .ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _pedidoService.MarcarComoEntregue(Guid.NewGuid(), _usuarioId));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Never);
    }

    [Fact]
    public async Task Dado_PedidoCancelado_Quando_MarcarComoEntregue_Entao_DeveLancarInvalidOperationExceptionSemSalvar()
    {
        var pedido = CriarPedidoPersistido(DateTime.UtcNow, PedidoStatus.Confirmado);
        pedido.Cancelar();
        _pedidoRepositoryMock.Setup(r => r.BuscarParaAlterar(pedido.PedidoId, _usuarioId)).ReturnsAsync(pedido);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _pedidoService.MarcarComoEntregue(pedido.PedidoId, _usuarioId));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(), Times.Never);
    }
}
