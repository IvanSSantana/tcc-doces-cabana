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

        _pedidoService = new PedidoService(
            _carrinhoServiceMock.Object,
            _itemCarrinhoRepositoryMock.Object,
            _enderecoServiceMock.Object,
            _freteServiceMock.Object,
            _pedidoRepositoryMock.Object,
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

    // ── Recusas ─────────────────────────────────────────────────────────

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
        // Plano §9: comparação sobre decimal, nunca double — 9.99 * 3 é um
        // caso clássico de perda de precisão se convertido para double.
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

    // ── Caminho feliz ───────────────────────────────────────────────────

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
        Assert.Equal(10m, pedidoGravado.Itens.First().PrecoUnitario); // RF-19: preço de agora, não o exibido
        Assert.Equal(28m, pedidoGravado.Valor); // 20 (produtos) + 8 (frete)
        Assert.Equal(8m, pedidoGravado.ValorDoFrete);
        Assert.NotNull(pagamentoGravado);
        Assert.Equal(MetodoPagamento.Pix, pagamentoGravado!.Metodo);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(itemDoCarrinho), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_PrecoDoProdutoMudouAposRevisao_Quando_FecharComValorDeAgora_Entao_DevePersistirOPrecoCongelado()
    {
        // RF-19/CA-12: o preço gravado no ItemPedido é o de agora
        // (linha.PrecoUnitario), nunca o que veio no formulário.
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
}
