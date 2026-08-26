using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class CarrinhoServiceTests
{
    private readonly Mock<IItemCarrinhoRepository> _itemCarrinhoRepositoryMock;
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CarrinhoService _carrinhoService;

    public CarrinhoServiceTests()
    {
        _itemCarrinhoRepositoryMock = new Mock<IItemCarrinhoRepository>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _carrinhoService = new CarrinhoService(
            _itemCarrinhoRepositoryMock.Object,
            _produtoRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Produto CriarProduto(ProdutoStatus status = ProdutoStatus.Ativo, decimal preco = 10m) =>
        new(Guid.NewGuid(), "Brigadeiro", preco, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m, status);

    // ── Acrescentar ─────────────────────────────────────────────────────

    [Fact]
    public async Task Dado_ProdutoDisponivelAindaNaoNoCarrinho_Quando_Acrescentar_Entao_DeveAdicionarNovoItem()
    {
        var produto = CriarProduto();
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produto.ProdutoId)).ReturnsAsync((ItemCarrinho?)null);

        await _carrinhoService.Acrescentar(usuarioId, produto.ProdutoId, 2);

        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(
            It.Is<ItemCarrinho>(i => i.UsuarioId == usuarioId && i.ProdutoId == produto.ProdutoId && i.Quantidade == 2)),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ProdutoJaNoCarrinho_Quando_AcrescentarDeNovo_Entao_DeveSomarNumaLinhaSo()
    {
        // RF-03/RN-01: acrescentar o que já está soma, não duplica.
        var produto = CriarProduto();
        var usuarioId = Guid.NewGuid();
        var itemExistente = new ItemCarrinho(usuarioId, produto.ProdutoId, 2);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produto.ProdutoId)).ReturnsAsync(itemExistente);

        await _carrinhoService.Acrescentar(usuarioId, produto.ProdutoId, 3);

        Assert.Equal(5, itemExistente.Quantidade);
        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<ItemCarrinho>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_SomaUltrapassariaOTeto_Quando_Acrescentar_Entao_DeveLimitarA99()
    {
        var produto = CriarProduto();
        var usuarioId = Guid.NewGuid();
        var itemExistente = new ItemCarrinho(usuarioId, produto.ProdutoId, 95);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produto.ProdutoId)).ReturnsAsync(itemExistente);

        await _carrinhoService.Acrescentar(usuarioId, produto.ProdutoId, 10);

        Assert.Equal(99, itemExistente.Quantidade);
    }

    [Fact]
    public async Task Dado_ProdutoInexistente_Quando_Acrescentar_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoId)).ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carrinhoService.Acrescentar(usuarioId, produtoId, 1));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    [Theory]
    [InlineData(ProdutoStatus.Inativo)]
    [InlineData(ProdutoStatus.ForaDeEstoque)]
    public async Task Dado_ProdutoIndisponivel_Quando_Acrescentar_Entao_DeveLancarInvalidOperationException(ProdutoStatus status)
    {
        // RN-06: os dois motivos são igualmente incompráveis (RF-04).
        var produto = CriarProduto(status);
        var usuarioId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _carrinhoService.Acrescentar(usuarioId, produto.ProdutoId, 1));

        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    // ── AlterarQuantidade ───────────────────────────────────────────────

    [Fact]
    public async Task Dado_ItemNoCarrinho_Quando_AlterarQuantidade_Entao_DeveAtualizar()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var item = new ItemCarrinho(usuarioId, produtoId, 1);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(item);

        await _carrinhoService.AlterarQuantidade(usuarioId, produtoId, 7);

        Assert.Equal(7, item.Quantidade);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_QuantidadeAlemDoTeto_Quando_AlterarQuantidade_Entao_DeveLimitarA99()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var item = new ItemCarrinho(usuarioId, produtoId, 1);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(item);

        await _carrinhoService.AlterarQuantidade(usuarioId, produtoId, 150);

        Assert.Equal(99, item.Quantidade);
    }

    [Fact]
    public async Task Dado_QuantidadeAbaixoDeUm_Quando_AlterarQuantidade_Entao_DeveRemoverOItem()
    {
        // RN-02: reduzir abaixo de 1 remove o item.
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var item = new ItemCarrinho(usuarioId, produtoId, 1);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(item);

        await _carrinhoService.AlterarQuantidade(usuarioId, produtoId, 0);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ItemInexistente_Quando_AlterarQuantidade_Entao_DeveLancarKeyNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync((ItemCarrinho?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carrinhoService.AlterarQuantidade(usuarioId, produtoId, 5));
    }

    // ── Remover ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Dado_ItemNoCarrinho_Quando_Remover_Entao_DeveRemover()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var item = new ItemCarrinho(usuarioId, produtoId, 1);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(item);

        await _carrinhoService.Remover(usuarioId, produtoId);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(item), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ItemInexistente_Quando_Remover_Entao_DeveLancarKeyNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync((ItemCarrinho?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carrinhoService.Remover(usuarioId, produtoId));
    }

    // ── ObterDoUsuario / subtotal ───────────────────────────────────────

    [Fact]
    public async Task Dado_CarrinhoComItensDisponiveis_Quando_ObterDoUsuario_Entao_SubtotalDeveSomarTudo()
    {
        var usuarioId = Guid.NewGuid();
        var produtoUm = CriarProduto(preco: 10m);
        var produtoDois = CriarProduto(preco: 5m);
        var itemUm = new ItemCarrinho(usuarioId, produtoUm.ProdutoId, 2);
        var itemDois = new ItemCarrinho(usuarioId, produtoDois.ProdutoId, 3);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(itemUm, produtoUm);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(itemDois, produtoDois);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([itemUm, itemDois]);

        var carrinho = await _carrinhoService.ObterDoUsuario(usuarioId);

        // 10*2 + 5*3 = 35
        Assert.Equal(35m, carrinho.Subtotal);
        Assert.Equal(5, carrinho.TotalDeItens);
    }

    [Fact]
    public async Task Dado_ItemIndisponivel_Quando_ObterDoUsuario_Entao_NaoDeveSomarNoSubtotal()
    {
        // RF-17/RN-06.
        var usuarioId = Guid.NewGuid();
        var produtoDisponivel = CriarProduto(ProdutoStatus.Ativo, preco: 10m);
        var produtoIndisponivel = CriarProduto(ProdutoStatus.Inativo, preco: 20m);
        var itemDisponivel = new ItemCarrinho(usuarioId, produtoDisponivel.ProdutoId, 1);
        var itemIndisponivel = new ItemCarrinho(usuarioId, produtoIndisponivel.ProdutoId, 1);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(itemDisponivel, produtoDisponivel);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(itemIndisponivel, produtoIndisponivel);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([itemDisponivel, itemIndisponivel]);

        var carrinho = await _carrinhoService.ObterDoUsuario(usuarioId);

        Assert.Equal(10m, carrinho.Subtotal);
        // Mas o item continua na lista, sinalizado (RN-07) — não some.
        Assert.Equal(2, carrinho.Linhas.Count);
        var linhaIndisponivel = carrinho.Linhas.Single(l => l.ProdutoId == produtoIndisponivel.ProdutoId);
        Assert.Equal(MotivoIndisponibilidade.ForaDoCatalogo, linhaIndisponivel.MotivoIndisponibilidade);
        Assert.False(linhaIndisponivel.Disponivel);
    }

    [Fact]
    public async Task Dado_ItemForaDeEstoque_Quando_ObterDoUsuario_Entao_MotivoDeveSerForaDeEstoque()
    {
        var usuarioId = Guid.NewGuid();
        var produto = CriarProduto(ProdutoStatus.ForaDeEstoque);
        var item = new ItemCarrinho(usuarioId, produto.ProdutoId, 1);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(item, produto);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([item]);

        var carrinho = await _carrinhoService.ObterDoUsuario(usuarioId);

        Assert.Equal(MotivoIndisponibilidade.ForaDeEstoque, carrinho.Linhas.Single().MotivoIndisponibilidade);
    }

    [Fact]
    public async Task Dado_PrecoDoProdutoMudou_Quando_ObterDoUsuario_Entao_DeveUsarOPrecoAtual()
    {
        // RN-04: o carrinho não tem coluna de preço — sempre lê do produto.
        var usuarioId = Guid.NewGuid();
        var produto = CriarProduto(preco: 25m);
        var item = new ItemCarrinho(usuarioId, produto.ProdutoId, 1);
        typeof(ItemCarrinho).GetProperty(nameof(ItemCarrinho.Produto))!.SetValue(item, produto);
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([item]);

        var carrinho = await _carrinhoService.ObterDoUsuario(usuarioId);

        Assert.Equal(25m, carrinho.Linhas.Single().PrecoUnitario);
    }

    // ── ContarItens ─────────────────────────────────────────────────────

    [Fact]
    public async Task Dado_Usuario_Quando_ContarItens_Entao_DeveRepassarAoRepositorio()
    {
        var usuarioId = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.ContarItens(usuarioId)).ReturnsAsync(7);

        var total = await _carrinhoService.ContarItens(usuarioId);

        Assert.Equal(7, total);
    }

    // ── Avulso (Fase 6) — mesmas regras da versão persistida, sobre uma
    // lista em vez do banco. ────────────────────────────────────────────

    [Fact]
    public async Task Dado_ProdutoDisponivelAindaNaoNaLista_Quando_AcrescentarAvulso_Entao_DeveAdicionarNovoItem()
    {
        var produto = CriarProduto();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);

        var resultado = await _carrinhoService.AcrescentarAvulso([], produto.ProdutoId, 2);

        var item = Assert.Single(resultado);
        Assert.Equal(produto.ProdutoId, item.ProdutoId);
        Assert.Equal(2, item.Quantidade);
    }

    [Fact]
    public async Task Dado_ProdutoJaNaLista_Quando_AcrescentarAvulsoDeNovo_Entao_DeveSomarNumaLinhaSo()
    {
        var produto = CriarProduto();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        var itens = new List<ItemDoCarrinhoDTO> { new(produto.ProdutoId, 2) };

        var resultado = await _carrinhoService.AcrescentarAvulso(itens, produto.ProdutoId, 3);

        var item = Assert.Single(resultado);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public async Task Dado_SomaUltrapassariaOTeto_Quando_AcrescentarAvulso_Entao_DeveLimitarA99()
    {
        var produto = CriarProduto();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);
        var itens = new List<ItemDoCarrinhoDTO> { new(produto.ProdutoId, 95) };

        var resultado = await _carrinhoService.AcrescentarAvulso(itens, produto.ProdutoId, 10);

        Assert.Equal(99, resultado.Single().Quantidade);
    }

    [Fact]
    public async Task Dado_ProdutoInexistente_Quando_AcrescentarAvulso_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoId)).ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carrinhoService.AcrescentarAvulso([], produtoId, 1));
    }

    [Theory]
    [InlineData(ProdutoStatus.Inativo)]
    [InlineData(ProdutoStatus.ForaDeEstoque)]
    public async Task Dado_ProdutoIndisponivel_Quando_AcrescentarAvulso_Entao_DeveLancarInvalidOperationException(ProdutoStatus status)
    {
        var produto = CriarProduto(status);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produto.ProdutoId)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _carrinhoService.AcrescentarAvulso([], produto.ProdutoId, 1));
    }

    [Fact]
    public void Dado_ItemNaLista_Quando_AlterarQuantidadeAvulsa_Entao_DeveAtualizar()
    {
        var produtoId = Guid.NewGuid();
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoId, 1) };

        var resultado = _carrinhoService.AlterarQuantidadeAvulsa(itens, produtoId, 7);

        Assert.Equal(7, resultado.Single().Quantidade);
    }

    [Fact]
    public void Dado_QuantidadeAlemDoTeto_Quando_AlterarQuantidadeAvulsa_Entao_DeveLimitarA99()
    {
        var produtoId = Guid.NewGuid();
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoId, 1) };

        var resultado = _carrinhoService.AlterarQuantidadeAvulsa(itens, produtoId, 150);

        Assert.Equal(99, resultado.Single().Quantidade);
    }

    [Fact]
    public void Dado_QuantidadeAbaixoDeUm_Quando_AlterarQuantidadeAvulsa_Entao_DeveRemoverOItem()
    {
        var produtoId = Guid.NewGuid();
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoId, 1) };

        var resultado = _carrinhoService.AlterarQuantidadeAvulsa(itens, produtoId, 0);

        Assert.Empty(resultado);
    }

    [Fact]
    public void Dado_ItemAusenteDaLista_Quando_AlterarQuantidadeAvulsa_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(() => _carrinhoService.AlterarQuantidadeAvulsa([], produtoId, 5));
    }

    [Fact]
    public void Dado_ItemNaLista_Quando_RemoverAvulso_Entao_DeveSairDaLista()
    {
        var produtoId = Guid.NewGuid();
        var outroProdutoId = Guid.NewGuid();
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoId, 1), new(outroProdutoId, 2) };

        var resultado = _carrinhoService.RemoverAvulso(itens, produtoId);

        var restante = Assert.Single(resultado);
        Assert.Equal(outroProdutoId, restante.ProdutoId);
    }

    [Fact]
    public void Dado_ItemAusenteDaLista_Quando_RemoverAvulso_Entao_DeveLancarKeyNotFoundException()
    {
        var produtoId = Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(() => _carrinhoService.RemoverAvulso([], produtoId));
    }

    [Fact]
    public async Task Dado_ListaComItensDisponiveis_Quando_MontarAvulso_Entao_SubtotalDeveSomarTudo()
    {
        var produtoUm = CriarProduto(preco: 10m);
        var produtoDois = CriarProduto(preco: 5m);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoUm.ProdutoId)).ReturnsAsync(produtoUm);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoDois.ProdutoId)).ReturnsAsync(produtoDois);
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoUm.ProdutoId, 2), new(produtoDois.ProdutoId, 3) };

        var carrinho = await _carrinhoService.MontarAvulso(itens);

        // 10*2 + 5*3 = 35
        Assert.Equal(35m, carrinho.Subtotal);
        Assert.Equal(5, carrinho.TotalDeItens);
    }

    [Fact]
    public async Task Dado_ItemIndisponivelNaLista_Quando_MontarAvulso_Entao_NaoDeveSomarNoSubtotal()
    {
        var produtoDisponivel = CriarProduto(ProdutoStatus.Ativo, preco: 10m);
        var produtoIndisponivel = CriarProduto(ProdutoStatus.ForaDeEstoque, preco: 20m);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoDisponivel.ProdutoId)).ReturnsAsync(produtoDisponivel);
        _produtoRepositoryMock.Setup(r => r.BuscarPorId(produtoIndisponivel.ProdutoId)).ReturnsAsync(produtoIndisponivel);
        var itens = new List<ItemDoCarrinhoDTO> { new(produtoDisponivel.ProdutoId, 1), new(produtoIndisponivel.ProdutoId, 1) };

        var carrinho = await _carrinhoService.MontarAvulso(itens);

        Assert.Equal(10m, carrinho.Subtotal);
        Assert.Equal(2, carrinho.Linhas.Count);
    }

    // ── Fundir (Fase 7) — RN-05: as quantidades do mesmo produto se somam,
    // limitadas ao teto; o que só existia num dos lados entra também. ───

    [Fact]
    public async Task Dado_ProdutoJaNoCarrinhoGuardado_Quando_Fundir_Entao_DeveSomarAsQuantidades()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var itemGuardado = new ItemCarrinho(usuarioId, produtoId, 3);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(itemGuardado);
        var itensDaSessao = new List<ItemDoCarrinhoDTO> { new(produtoId, 2) };

        await _carrinhoService.Fundir(usuarioId, itensDaSessao);

        // CA-13: 3 (guardado) + 2 (avulso) = 5.
        Assert.Equal(5, itemGuardado.Quantidade);
        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<ItemCarrinho>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_ProdutoSoNaSessao_Quando_Fundir_Entao_DeveEntrarComoNovoItem()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync((ItemCarrinho?)null);
        var itensDaSessao = new List<ItemDoCarrinhoDTO> { new(produtoId, 4) };

        await _carrinhoService.Fundir(usuarioId, itensDaSessao);

        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(
            It.Is<ItemCarrinho>(i => i.UsuarioId == usuarioId && i.ProdutoId == produtoId && i.Quantidade == 4)),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_SomaUltrapassariaOTeto_Quando_Fundir_Entao_DeveLimitarA99()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var itemGuardado = new ItemCarrinho(usuarioId, produtoId, 95);
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoId)).ReturnsAsync(itemGuardado);
        var itensDaSessao = new List<ItemDoCarrinhoDTO> { new(produtoId, 10) };

        await _carrinhoService.Fundir(usuarioId, itensDaSessao);

        Assert.Equal(99, itemGuardado.Quantidade);
    }

    [Fact]
    public async Task Dado_ProdutosDiferentesNosDoisLados_Quando_Fundir_Entao_NenhumDeveDesaparecer()
    {
        var usuarioId = Guid.NewGuid();
        var produtoGuardado = Guid.NewGuid();
        var produtoAvulso = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.Buscar(usuarioId, produtoAvulso)).ReturnsAsync((ItemCarrinho?)null);
        var itensDaSessao = new List<ItemDoCarrinhoDTO> { new(produtoAvulso, 1) };

        await _carrinhoService.Fundir(usuarioId, itensDaSessao);

        // O item guardado (produtoGuardado) não é tocado — só o repositório
        // decide o que existe; aqui só provamos que o avulso entrou.
        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(
            It.Is<ItemCarrinho>(i => i.ProdutoId == produtoAvulso)), Times.Once);
    }

    [Fact]
    public async Task Dado_SessaoVazia_Quando_Fundir_Entao_NaoDeveSalvarNadaNemAdicionarNada()
    {
        var usuarioId = Guid.NewGuid();

        await _carrinhoService.Fundir(usuarioId, []);

        _itemCarrinhoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<ItemCarrinho>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }

    // ── Esvaziar (spec 021) ──────────────────────────────────────────────

    [Fact]
    public async Task Dado_CarrinhoComItens_Quando_Esvaziar_Entao_DeveRemoverTodosEChamarSalvarAlteracoesUmaVez()
    {
        var usuarioId = Guid.NewGuid();
        var itens = new List<ItemCarrinho>
        {
            new(usuarioId, Guid.NewGuid(), 2),
            new(usuarioId, Guid.NewGuid(), 1),
        };
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync(itens);

        await _carrinhoService.Esvaziar(usuarioId);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(itens[0]), Times.Once);
        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(itens[1]), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_CarrinhoJaVazio_Quando_Esvaziar_Entao_NaoDeveQuebrarNemChamarSalvarAlteracoes()
    {
        var usuarioId = Guid.NewGuid();
        _itemCarrinhoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([]);

        await _carrinhoService.Esvaziar(usuarioId);

        _itemCarrinhoRepositoryMock.Verify(r => r.Remover(It.IsAny<ItemCarrinho>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }
}
