using System.Text;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;
using DocesCabana.MVC.Models;
using AdminBannerController = DocesCabana.MVC.Areas.Admin.Controllers.BannerController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

public class BannerControllerTests
{
    private const string UrlEnviada = "https://projeto.supabase.co/storage/v1/object/public/images/public/banner.png";

    private readonly Mock<IBannerService> _bannerServiceMock = new();
    private readonly Mock<ICategoriaService> _categoriaServiceMock = new();
    private readonly Mock<IProdutoService> _produtoServiceMock = new();
    private readonly Mock<IArmazenamentoDeImagem> _armazenamentoMock = new();
    private readonly AdminBannerController _controller;

    public BannerControllerTests()
    {
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(
            [new() { CategoriaId = Guid.NewGuid(), Nome = "Doces", Apelido = "doces" }]);
        _produtoServiceMock.Setup(s => s.BuscarTodosProdutos()).ReturnsAsync(
        [
            new() { ProdutoId = Guid.NewGuid(), Nome = "Brigadeiro", Status = ProdutoStatus.Ativo },
            new() { ProdutoId = Guid.NewGuid(), Nome = "Beijinho", Status = ProdutoStatus.Inativo },
        ]);
        _armazenamentoMock
            .Setup(a => a.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaSucesso(UrlEnviada));

        var contextoHttp = new DefaultHttpContext();
        _controller = new AdminBannerController(
            _bannerServiceMock.Object, _categoriaServiceMock.Object, _produtoServiceMock.Object,
            _armazenamentoMock.Object, new ImagemParaEnvioDTOValidator())
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, Mock.Of<ITempDataProvider>())
        };
    }

    private static IFormFile CriarArquivo(string nome = "banner.png", string contentType = "image/png")
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(new string('x', 1024)));
        return new FormFile(stream, 0, stream.Length, "imagem", nome) { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    private static BannerDTO DtoValido() => new()
    {
        BannerId = Guid.NewGuid(),
        TextoAlternativo = "Natal na Cabana",
        Ordem = 0,
        DataInicio = new DateOnly(2026, 12, 1),
        TipoDeDestino = TipoDeDestinoDoBanner.Categoria,
        CategoriaId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Dado_BannersCadastrados_Quando_Index_Entao_DeveRetornarViewComALista()
    {
        _bannerServiceMock.Setup(s => s.ListarParaGestao()).ReturnsAsync([]);

        var resultado = await _controller.Index();

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.IsAssignableFrom<IReadOnlyList<ResumoDeBannerDTO>>(view.Model);
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveOferecerSoProdutosAtivos()
    {
        var resultado = await _controller.Cadastro();

        var modelo = Assert.IsType<FormularioDeBannerViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.True(modelo.EhNovo);
        Assert.Equal("Brigadeiro", Assert.Single(modelo.Produtos).Nome);
        Assert.Single(modelo.Categorias);
    }

    [Fact]
    public async Task Dado_SemImagem_Quando_CadastroPost_Entao_DeveVoltarAoFormularioComErroNaImagem()
    {
        var resultado = await _controller.Cadastro(DtoValido(), null);

        Assert.IsType<ViewResult>(resultado);
        Assert.True(_controller.ModelState.ContainsKey("imagem"));
        _bannerServiceMock.Verify(s => s.Cadastrar(It.IsAny<BannerDTO>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ArquivoQueNaoEImagem_Quando_CadastroPost_Entao_DeveVoltarComErroSemEnviar()
    {
        var resultado = await _controller.Cadastro(DtoValido(), CriarArquivo("planilha.csv", "text/csv"));

        Assert.IsType<ViewResult>(resultado);
        Assert.True(_controller.ModelState.ContainsKey("imagem"));
        _armazenamentoMock.Verify(a => a.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_NaoDeveEnviarImagemNemCadastrar()
    {
        _controller.ModelState.AddModelError("Banner.TextoAlternativo", "Descreva a imagem do banner.");

        var resultado = await _controller.Cadastro(DtoValido(), CriarArquivo());

        var modelo = Assert.IsType<FormularioDeBannerViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.Equal("Natal na Cabana", modelo.Banner.TextoAlternativo);
        _armazenamentoMock.Verify(a => a.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _bannerServiceMock.Verify(s => s.Cadastrar(It.IsAny<BannerDTO>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_FalhaNoEnvioDaImagem_Quando_CadastroPost_Entao_DeveVoltarComErroSemCadastrar()
    {
        _armazenamentoMock
            .Setup(a => a.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaFalha("Não foi possível enviar a imagem agora."));

        var resultado = await _controller.Cadastro(DtoValido(), CriarArquivo());

        Assert.IsType<ViewResult>(resultado);
        Assert.Contains("Não foi possível enviar a imagem agora.",
            _controller.ModelState[string.Empty]!.Errors.Select(e => e.ErrorMessage));
        _bannerServiceMock.Verify(s => s.Cadastrar(It.IsAny<BannerDTO>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_DadosEImagemValidos_Quando_CadastroPost_Entao_DeveCadastrarComAUrlERedirecionar()
    {
        var dto = DtoValido();

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(AdminBannerController.Index), redirecionamento.ActionName);
        _bannerServiceMock.Verify(s => s.Cadastrar(dto, UrlEnviada), Times.Once);
    }

    [Fact]
    public async Task Dado_BannerExistente_Quando_EdicaoGet_Entao_DeveTrazerOsDadosNoFormulario()
    {
        var dto = DtoValido();
        _bannerServiceMock.Setup(s => s.BuscarParaEdicao(dto.BannerId)).ReturnsAsync(dto);

        var resultado = await _controller.Edicao(dto.BannerId);

        var modelo = Assert.IsType<FormularioDeBannerViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.False(modelo.EhNovo);
        Assert.Same(dto, modelo.Banner);
    }

    [Fact]
    public async Task Dado_EdicaoSemImagemNova_Quando_EdicaoPost_Entao_DeveAtualizarSemTrocarImagemERedirecionar()
    {
        var dto = DtoValido();

        var resultado = await _controller.Edicao(dto, null);

        Assert.IsType<RedirectToActionResult>(resultado);
        _bannerServiceMock.Verify(s => s.Atualizar(dto, null), Times.Once);
        _armazenamentoMock.Verify(a => a.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EdicaoComImagemNova_Quando_EdicaoPost_Entao_DeveAtualizarComAUrlNova()
    {
        var dto = DtoValido();

        await _controller.Edicao(dto, CriarArquivo());

        _bannerServiceMock.Verify(s => s.Atualizar(dto, UrlEnviada), Times.Once);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_EdicaoPost_Entao_DeveVoltarComAImagemAtual()
    {
        var dto = DtoValido();
        _bannerServiceMock.Setup(s => s.BuscarParaEdicao(dto.BannerId))
            .ReturnsAsync(new BannerDTO { BannerId = dto.BannerId, ImagemUrl = "https://exemplo.com/atual.png" });
        _controller.ModelState.AddModelError("Banner.Ordem", "Informe a ordem do banner.");

        var resultado = await _controller.Edicao(dto, null);

        var modelo = Assert.IsType<FormularioDeBannerViewModel>(Assert.IsType<ViewResult>(resultado).Model);
        Assert.Equal("https://exemplo.com/atual.png", modelo.Banner.ImagemUrl);
        _bannerServiceMock.Verify(s => s.Atualizar(It.IsAny<BannerDTO>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Dado_BannerAtivo_Quando_Desativar_Entao_DeveDesativarERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Desativar(id);

        Assert.IsType<RedirectToActionResult>(resultado);
        _bannerServiceMock.Verify(s => s.Desativar(id), Times.Once);
    }

    [Fact]
    public async Task Dado_BannerDesativado_Quando_Ativar_Entao_DeveAtivarERedirecionar()
    {
        var id = Guid.NewGuid();

        var resultado = await _controller.Ativar(id);

        Assert.IsType<RedirectToActionResult>(resultado);
        _bannerServiceMock.Verify(s => s.Ativar(id), Times.Once);
    }
}
