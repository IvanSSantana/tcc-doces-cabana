using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;
using AdminProdutoController = DocesCabana.MVC.Areas.Admin.Controllers.ProdutoController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

// Era CatalogoControllerTests (010). Renomeado para acompanhar
// Areas/Admin/Controllers/ProdutoController (011, RQ-04). O alias evita
// colidir com o DocesCabana.MVC.Controllers.ProdutoController público, que
// já tem teste próprio em Units/Controllers/ProdutoControllerTests.cs.
public class ProdutoControllerTests
{
    private readonly Mock<IProdutoService> _produtoServiceMock;
    private readonly Mock<ICategoriaService> _categoriaServiceMock;
    private readonly Mock<IArmazenamentoDeImagem> _armazenamentoMock;
    private readonly AdminProdutoController _controller;

    public ProdutoControllerTests()
    {
        _produtoServiceMock = new Mock<IProdutoService>();
        _categoriaServiceMock = new Mock<ICategoriaService>();
        _armazenamentoMock = new Mock<IArmazenamentoDeImagem>();
        _controller = new AdminProdutoController(
            _produtoServiceMock.Object, _categoriaServiceMock.Object, _armazenamentoMock.Object, new ImagemParaEnvioDTOValidator());
    }

    // FormFile concreto (Microsoft.AspNetCore.Http) — sem lib nova, sem
    // arquivo em disco: o conteúdo é uma stream em memória.
    private static IFormFile CriarArquivo(string nome = "brigadeiro.jpg", string contentType = "image/jpeg", int tamanhoEmBytes = 1024)
    {
        var stream = new MemoryStream(new byte[tamanhoEmBytes]);
        return new FormFile(stream, 0, stream.Length, "imagem", nome) { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    [Fact]
    public async Task Dado_RequisicaoValida_Quando_CadastroGet_Entao_DeveCarregarSubcategoriasERetornarView()
    {
        var categorias = new List<CategoriaDTO>
        {
            new()
            {
                CategoriaId = Guid.NewGuid(),
                Nome = "Doces",
                Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = Guid.NewGuid(), Nome = "Barras" }]
            }
        };
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(categorias);

        var resultado = await _controller.Cadastro();

        Assert.IsType<ViewResult>(resultado);
        _categoriaServiceMock.Verify(s => s.ListarComSubcategorias(), Times.Once);
    }

    [Fact]
    public async Task Dado_SubcategoriaComMesmoNomeEmDuasCategorias_Quando_CadastroGet_Entao_DeveQualificarPelaCategoria()
    {
        // RF-28/CA-24: "Cappuccino" existe em Doces e em Empório. O seletor
        // precisa distinguir as duas, não listar "Cappuccino" duas vezes.
        var idCappuccinoDoces = Guid.NewGuid();
        var idCappuccinoEmporio = Guid.NewGuid();
        var categorias = new List<CategoriaDTO>
        {
            new() { CategoriaId = Guid.NewGuid(), Nome = "Doces", Apelido = "doces",
                Subcategorias = [new() { SubcategoriaId = idCappuccinoDoces, Nome = "Cappuccino" }] },
            new() { CategoriaId = Guid.NewGuid(), Nome = "Empório", Apelido = "emporio",
                Subcategorias = [new() { SubcategoriaId = idCappuccinoEmporio, Nome = "Cappuccino" }] },
        };
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(categorias);

        var resultado = await _controller.Cadastro();

        var viewResult = Assert.IsType<ViewResult>(resultado);
        var opcoes = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.Rendering.SelectList>(viewResult.ViewData["Subcategorias"]);
        var rotulos = opcoes.Cast<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>().Select(i => i.Text).ToList();

        Assert.Contains("Doces › Cappuccino", rotulos);
        Assert.Contains("Empório › Cappuccino", rotulos);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarViewComDtoSemChamarServico()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new ProdutoDTO();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(dto, viewResult.Model);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // RF-02, CA-02: sem arquivo nenhum, o produto não é cadastrado e o
    // armazenamento nunca é chamado.
    [Fact]
    public async Task Dado_SemArquivo_Quando_CadastroPost_Entao_DeveInvalidarModelStateSemChamarArmazenamento()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, imagem: null);

        Assert.IsType<ViewResult>(resultado);
        Assert.False(_controller.ModelState.IsValid);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    // RF-03/RF-04, CA-03/CA-04: arquivo recusado pelo validador (formato ou
    // tamanho) tem o mesmo efeito — nunca chega a chamar o armazenamento.
    [Fact]
    public async Task Dado_ArquivoDeFormatoInvalido_Quando_CadastroPost_Entao_DeveInvalidarModelStateSemChamarArmazenamento()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, CriarArquivo(nome: "documento.pdf", contentType: "application/pdf"));

        Assert.IsType<ViewResult>(resultado);
        Assert.False(_controller.ModelState.IsValid);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    // RF-08, CA-08: envio falhou → volta à view e o cadastro nunca roda.
    [Fact]
    public async Task Dado_EnvioDaImagemFalhou_Quando_CadastroPost_Entao_DeveVoltarAViewSemCadastrar()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());
        _armazenamentoMock.Setup(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaFalha("Não foi possível enviar a imagem agora."));

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        Assert.IsType<ViewResult>(resultado);
        Assert.False(_controller.ModelState.IsValid);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    // CA-06: o endereço devolvido pelo armazenamento chega ao DTO gravado, e
    // a ação redireciona.
    [Fact]
    public async Task Dado_ProdutoValido_Quando_CadastroPost_Entao_DeveCadastrarComOEnderecoDoEnvioERedirecionarComConfirmacao()
    {
        var dto = CriarDtoValido();
        _armazenamentoMock.Setup(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaSucesso("https://projeto.supabase.co/storage/v1/object/public/images/public/abc.jpg"));
        _produtoServiceMock.Setup(s => s.Cadastrar(It.Is<ProdutoDTO>(d => d.ImagemUrl == "https://projeto.supabase.co/storage/v1/object/public/images/public/abc.jpg")))
            .ReturnsAsync(dto);

        ConfigurarTempData();

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Cadastro", redirectResult.ActionName);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.Is<ProdutoDTO>(d => d.ImagemUrl == "https://projeto.supabase.co/storage/v1/object/public/images/public/abc.jpg")), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    private static ProdutoDTO CriarDtoValido() => new()
    {
        Nome = "Brigadeiro Gourmet",
        Preco = 5.50m,
        Status = ProdutoStatus.Ativo,
        SubcategoriaId = Guid.NewGuid(),
        Peso = 0.5m,
        Altura = 10m,
        Largura = 15m,
        Comprimento = 20m
    };

    private void ConfigurarTempData()
    {
        var httpContext = new DefaultHttpContext();
        var tempDataProviderMock = new Mock<ITempDataProvider>();
        tempDataProviderMock.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());

        _controller.TempData = new TempDataDictionary(httpContext, tempDataProviderMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
