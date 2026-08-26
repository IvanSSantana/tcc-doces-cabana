using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Validators;
using DocesCabana.MVC.Controllers;
using DocesCabana.MVC.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DocesCabana.Tests.Units.Controllers;

public class CarrinhoControllerTests
{
    private readonly Mock<ICarrinhoService> _carrinhoServiceMock;
    private readonly Mock<IFreteService> _freteServiceMock;
    private readonly Mock<IPedidoService> _pedidoServiceMock;
    private readonly Mock<IEnderecoService> _enderecoServiceMock;
    private readonly CarrinhoController _controller;

    public CarrinhoControllerTests()
    {
        _carrinhoServiceMock = new Mock<ICarrinhoService>();
        _freteServiceMock = new Mock<IFreteService>();
        _pedidoServiceMock = new Mock<IPedidoService>();
        _enderecoServiceMock = new Mock<IEnderecoService>();
        // Padrão para todo teste que chega a montar a tela — não é o foco
        // da maioria deles; os que testam o passo em si sobrescrevem.
        _pedidoServiceMock
            .Setup(s => s.MontarPasso(It.IsAny<PassoDoFechamento>(), It.IsAny<CarrinhoDTO>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>()))
            .ReturnsAsync(new PassoDoFechamentoDTO());
        var httpContext = new DefaultHttpContext { Session = new SessaoFalsa() };
        // O validador é o de verdade, não mockado (spec 020): é lógica pura,
        // sem dependência externa — mockar esconderia o próprio
        // comportamento que os testes de CEP inválido/válido querem provar.
        _controller = new CarrinhoController(
            _carrinhoServiceMock.Object, _freteServiceMock.Object, new ConsultaDeFreteDTOValidator(),
            _pedidoServiceMock.Object, _enderecoServiceMock.Object)
        {
            // Visitante anônimo por padrão; ConfigurarUsuarioAutenticado
            // substitui isto nos testes que precisam de um usuário logado.
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Index_Entao_DeveUsarOServicoEDevolverView()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var carrinho = new CarrinhoDTO();
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(carrinho);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(carrinho, viewResult.Model);
        _carrinhoServiceMock.Verify(s => s.ObterDoUsuario(usuarioId), Times.Once);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Index_Entao_DeveDevolverPartialView()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Index();

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ItensDoCarrinho", partial.ViewName);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Acrescentar_Entao_DeveChamarOServicoERedirecionar()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 3);

        _carrinhoServiceMock.Verify(s => s.Acrescentar(usuarioId, produtoId, 3), Times.Once);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_QuantidadeOmitida_Quando_Acrescentar_Entao_DeveUsarUma()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.Acrescentar(produtoId);

        _carrinhoServiceMock.Verify(s => s.Acrescentar(usuarioId, produtoId, 1), Times.Once);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Acrescentar_Entao_DeveDevolverPartialView()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 1);

        var partial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ItensDoCarrinho", partial.ViewName);
    }

    // Fase 6 (spec 017): o visitante também acrescenta — não há mais
    // desafio de login nas ações de escrita. O carrinho dele vive na
    // sessão, não no banco (CA-12).

    [Fact]
    public async Task Dado_Visitante_Quando_Acrescentar_Entao_DeveGuardarNaSessaoENaoUsarOBanco()
    {
        var produtoId = Guid.NewGuid();
        _carrinhoServiceMock
            .Setup(s => s.AcrescentarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>(), produtoId, (short)2))
            .ReturnsAsync([new ItemDoCarrinhoDTO(produtoId, 2)]);
        _carrinhoServiceMock.Setup(s => s.MontarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>())).ReturnsAsync(new CarrinhoDTO());

        var resultado = await _controller.Acrescentar(produtoId, 2);

        _carrinhoServiceMock.Verify(s => s.AcrescentarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>(), produtoId, (short)2), Times.Once);
        _carrinhoServiceMock.Verify(s => s.Acrescentar(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Dado_Visitante_Quando_Index_Entao_DeveMontarDaSessao()
    {
        var carrinho = new CarrinhoDTO();
        _carrinhoServiceMock.Setup(s => s.MontarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>())).ReturnsAsync(carrinho);

        var resultado = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(carrinho, viewResult.Model);
        _carrinhoServiceMock.Verify(s => s.ObterDoUsuario(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_AlterarQuantidade_Entao_DeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.AlterarQuantidade(produtoId, 5);

        _carrinhoServiceMock.Verify(s => s.AlterarQuantidade(usuarioId, produtoId, 5), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Remover_Entao_DeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.Remover(produtoId);

        _carrinhoServiceMock.Verify(s => s.Remover(usuarioId, produtoId), Times.Once);
    }

    // ── Esvaziar (spec 021) ──────────────────────────────────────────────

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Esvaziar_Entao_DeveChamarOServicoERedirecionar()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);

        var resultado = await _controller.Esvaziar();

        _carrinhoServiceMock.Verify(s => s.Esvaziar(usuarioId), Times.Once);
        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(CarrinhoController.Index), redirecionamento.ActionName);
    }

    [Fact]
    public async Task Dado_Visitante_Quando_Esvaziar_Entao_DeveLimparASessaoENaoUsarOBanco()
    {
        HttpContext.Session.Escrever([new ItemDoCarrinhoDTO(Guid.NewGuid(), 2)]);

        var resultado = await _controller.Esvaziar();

        _carrinhoServiceMock.Verify(s => s.Esvaziar(It.IsAny<Guid>()), Times.Never);
        Assert.Empty(HttpContext.Session.Ler());
        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public void Dado_QualquerRequisicao_Quando_ConfirmarEsvaziar_Entao_DeveDevolverAView()
    {
        var resultado = _controller.ConfirmarEsvaziar();

        Assert.IsType<ViewResult>(resultado);
    }

    // ── Cotação de frete (spec 020) ──────────────────────────────────────

    private LinhaDoCarrinhoDTO CriarLinha(
        decimal preco = 10m, short quantidade = 1,
        DocesCabana.Application.Enums.MotivoIndisponibilidade motivo = DocesCabana.Application.Enums.MotivoIndisponibilidade.Nenhum) => new()
    {
        ProdutoId = Guid.NewGuid(),
        Nome = "Brigadeiro",
        PrecoUnitario = preco,
        Quantidade = quantidade,
        ValorDaLinha = preco * quantidade,
        Peso = 0.5m,
        Altura = 10m,
        Largura = 15m,
        Comprimento = 20m,
        MotivoIndisponibilidade = motivo
    };

    [Fact]
    public async Task Dado_CepValidoEItemDisponivel_Quando_Index_Entao_DeveCotarEDevolverViewComCotacao()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var carrinho = new CarrinhoDTO { Linhas = [CriarLinha()] };
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(carrinho);
        var cotacao = new CotacaoDeFreteDTO("01310000", [new OpcaoDeFreteDTO(1, "Correios", "PAC", 18m, 8, 9)], null);
        _freteServiceMock.Setup(s => s.Cotar("01310000", It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>())).ReturnsAsync(cotacao);

        var resultado = await _controller.Index(cep: "01310000");

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<CarrinhoDTO>(viewResult.Model);
        Assert.True(modelo.TemEntregaCalculada);
    }

    [Fact]
    public async Task Dado_CepComFormatoInvalido_Quando_Index_Entao_NuncaDeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var carrinho = new CarrinhoDTO { Linhas = [CriarLinha()] };
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(carrinho);

        await _controller.Index(cep: "123");

        _freteServiceMock.Verify(
            s => s.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Never);
    }

    [Fact]
    public async Task Dado_CepComFormatoInvalido_Quando_Index_Entao_ModelStateDeveFicarInvalido()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO { Linhas = [CriarLinha()] });

        await _controller.Index(cep: "123");

        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Dado_CarrinhoSemItemDisponivel_Quando_Index_ComCep_Entao_NaoDeveCotar()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var linhaIndisponivel = CriarLinha(motivo: DocesCabana.Application.Enums.MotivoIndisponibilidade.ForaDeEstoque);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO { Linhas = [linhaIndisponivel] });

        await _controller.Index(cep: "01310000");

        _freteServiceMock.Verify(
            s => s.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ItemDisponivelEIndisponivel_Quando_Index_ComCep_Entao_DevePassarApenasODisponivelAoServico()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        var disponivel = CriarLinha();
        var indisponivel = CriarLinha(motivo: DocesCabana.Application.Enums.MotivoIndisponibilidade.ForaDeEstoque);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO { Linhas = [disponivel, indisponivel] });
        _freteServiceMock
            .Setup(s => s.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO("01310000", [], "indiferente"));

        await _controller.Index(cep: "01310000");

        _freteServiceMock.Verify(s => s.Cotar(
            "01310000",
            It.Is<IReadOnlyList<LinhaDoCarrinhoDTO>>(lista => lista.Count == 1 && lista[0].ProdutoId == disponivel.ProdutoId)),
            Times.Once);
    }

    // ── Passos do fechamento (spec 022) ──────────────────────────────────

    [Fact]
    public async Task Dado_PassoDesconhecidoNaQuerystring_Quando_Index_Entao_DeveCairNoPrimeiroPasso()
    {
        // "Desconhecido" aqui é qualquer coisa que não seja um dos quatro
        // nomes do enum — o próprio model binding do ASP.NET Core resolve
        // isso para o valor padrão do parâmetro (Carrinho, o primeiro
        // membro), sem passar pela action. Este teste prova só que, quando
        // isso acontece, a action monta o passo Carrinho normalmente.
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());

        await _controller.Index();

        _pedidoServiceMock.Verify(s => s.MontarPasso(
            PassoDoFechamento.Carrinho, It.IsAny<CarrinhoDTO>(), usuarioId, null, null), Times.Once);
    }

    [Fact]
    public async Task Dado_UsuarioAutenticado_Quando_Index_Entao_PassoDeContaNaoDeveSerOferecido()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());
        var passoDto = new PassoDoFechamentoDTO
        {
            PassosVisiveis = [PassoDoFechamento.Carrinho, PassoDoFechamento.Endereco, PassoDoFechamento.Pagamento]
        };
        _pedidoServiceMock
            .Setup(s => s.MontarPasso(It.IsAny<PassoDoFechamento>(), It.IsAny<CarrinhoDTO>(), usuarioId, It.IsAny<Guid?>(), It.IsAny<int?>()))
            .ReturnsAsync(passoDto);

        await _controller.Index();

        Assert.DoesNotContain(PassoDoFechamento.Conta, ((PassoDoFechamentoDTO)_controller.ViewData["PassoDoFechamento"]!).PassosVisiveis);
    }

    [Fact]
    public async Task Dado_VisitanteNavegandoDiretoParaEndereco_Quando_Index_Entao_DeveCairNoPassoDeConta()
    {
        // Sem autenticação, forçar ?passo=Endereco pela URL não deve
        // oferecer o conteúdo desse passo — cai no de conta (RF-02).
        _carrinhoServiceMock.Setup(s => s.MontarAvulso(It.IsAny<IReadOnlyList<ItemDoCarrinhoDTO>>())).ReturnsAsync(new CarrinhoDTO());

        await _controller.Index(passo: PassoDoFechamento.Endereco);

        _pedidoServiceMock.Verify(s => s.MontarPasso(
            PassoDoFechamento.Conta, It.IsAny<CarrinhoDTO>(), null, It.IsAny<Guid?>(), It.IsAny<int?>()), Times.Once);
    }

    // ── Cadastro de endereço no fechamento (spec 022, RF-07) ─────────────

    [Fact]
    public async Task Dado_EnderecoValido_Quando_CadastrarEndereco_Entao_DeveChamarOServicoERedirecionarParaOPassoDeEndereco()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());
        var dto = new EnderecoDTO { Estado = "SP", Cidade = "Cidade", Bairro = "Bairro", CEP = "17340001", Rua = "Rua", Numero = 1 };

        var resultado = await _controller.CadastrarEndereco(dto);

        _enderecoServiceMock.Verify(s => s.Cadastrar(dto, usuarioId), Times.Once);
        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(CarrinhoController.Index), redirecionamento.ActionName);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastrarEndereco_Entao_NuncaDeveChamarOServico()
    {
        var usuarioId = Guid.NewGuid();
        ConfigurarUsuarioAutenticado(usuarioId);
        _carrinhoServiceMock.Setup(s => s.ObterDoUsuario(usuarioId)).ReturnsAsync(new CarrinhoDTO());
        _controller.ModelState.AddModelError("CEP", "CEP é obrigatório!");

        await _controller.CadastrarEndereco(new EnderecoDTO());

        _enderecoServiceMock.Verify(s => s.Cadastrar(It.IsAny<EnderecoDTO>(), It.IsAny<Guid>()), Times.Never);
    }

    private HttpContext HttpContext => _controller.ControllerContext.HttpContext;

    private void ConfigurarUsuarioAutenticado(Guid usuarioId)
    {
        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())], "TesteAutenticacao");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identidade),
            Session = _controller.ControllerContext.HttpContext.Session,
        };

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    // ISession real precisa de um IDistributedCache configurado — pesado
    // demais para um teste de unidade que só quer provar que o controlador
    // lê e escreve na sessão certa. Um dicionário em memória basta.
    private sealed class SessaoFalsa : ISession
    {
        private readonly Dictionary<string, byte[]> _valores = new();

        public bool IsAvailable => true;
        public string Id => "sessao-de-teste";
        public IEnumerable<string> Keys => _valores.Keys;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
        public void Set(string key, byte[] value) => _valores[key] = value;
        public void Remove(string key) => _valores.Remove(key);
        public void Clear() => _valores.Clear();
    }
}
