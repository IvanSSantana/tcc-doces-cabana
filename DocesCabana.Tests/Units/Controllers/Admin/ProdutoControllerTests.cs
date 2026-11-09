using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.MVC.Models;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;
using AdminProdutoController = DocesCabana.MVC.Areas.Admin.Controllers.ProdutoController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DocesCabana.Tests.Units.Controllers.Admin;

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
            _produtoServiceMock.Object, _categoriaServiceMock.Object, _armazenamentoMock.Object, new ImagemParaEnvioDTOValidator())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

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

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_FormularioProduto", parcial.ViewName);
        _categoriaServiceMock.Verify(s => s.ListarComSubcategorias(), Times.Once);
    }

    [Fact]
    public async Task Dado_SubcategoriaComMesmoNomeEmDuasCategorias_Quando_CadastroGet_Entao_DeveQualificarPelaCategoria()
    {
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

        var viewResult = Assert.IsType<PartialViewResult>(resultado);
        var opcoes = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.Rendering.SelectList>(viewResult.ViewData["Subcategorias"]);
        var rotulos = opcoes.Cast<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>().Select(i => i.Text).ToList();

        Assert.Contains("Doces › Cappuccino", rotulos);
        Assert.Contains("Empório › Cappuccino", rotulos);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarPartialCom400SemChamarServico()
    {
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new ProdutoDTO();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal(400, parcial.StatusCode);
        Assert.Equal(dto, parcial.Model);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_SemArquivo_Quando_CadastroPost_Entao_DeveInvalidarModelStateSemChamarArmazenamento()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, imagem: null);

        Assert.Equal(400, Assert.IsType<PartialViewResult>(resultado).StatusCode);
        Assert.False(_controller.ModelState.IsValid);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ArquivoDeFormatoInvalido_Quando_CadastroPost_Entao_DeveInvalidarModelStateSemChamarArmazenamento()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Cadastro(dto, CriarArquivo(nome: "documento.pdf", contentType: "application/pdf"));

        Assert.Equal(400, Assert.IsType<PartialViewResult>(resultado).StatusCode);
        Assert.False(_controller.ModelState.IsValid);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EnvioDaImagemFalhou_Quando_CadastroPost_Entao_DeveVoltarAViewSemCadastrar()
    {
        var dto = CriarDtoValido();
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias())
            .ReturnsAsync(new List<CategoriaDTO>());
        _armazenamentoMock.Setup(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaFalha("Não foi possível enviar a imagem agora."));

        var resultado = await _controller.Cadastro(dto, CriarArquivo());

        Assert.Equal(400, Assert.IsType<PartialViewResult>(resultado).StatusCode);
        Assert.False(_controller.ModelState.IsValid);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

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
        Assert.Equal("Index", redirectResult.ActionName);
        _produtoServiceMock.Verify(s => s.Cadastrar(It.Is<ProdutoDTO>(d => d.ImagemUrl == "https://projeto.supabase.co/storage/v1/object/public/images/public/abc.jpg")), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_FiltroEPagina_Quando_Index_Entao_DeveRepassarEMontarOViewModel()
    {
        var pagina = new PaginaDeProdutosDTO { PaginaAtual = 2, TotalDePaginas = 3, TotalDeItens = 25 };
        _produtoServiceMock.Setup(s => s.ListarParaGestao(FiltroDeGestaoDeProduto.Inativos, 2)).ReturnsAsync(pagina);

        var resultado = await _controller.Index(FiltroDeGestaoDeProduto.Inativos, 2);

        var view = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<GestaoDeProdutosViewModel>(view.Model);
        Assert.Same(pagina, modelo.Pagina);
        Assert.Equal(FiltroDeGestaoDeProduto.Inativos, modelo.Filtro);
        _produtoServiceMock.Verify(s => s.ListarParaGestao(FiltroDeGestaoDeProduto.Inativos, 2), Times.Once);
    }

    [Fact]
    public async Task Dado_RequisicaoAssincrona_Quando_Index_Entao_DeveRetornarSoOResultadoEmPartial()
    {
        var pagina = new PaginaDeProdutosDTO { PaginaAtual = 2, TotalDePaginas = 3 };
        _produtoServiceMock.Setup(s => s.ListarParaGestao(FiltroDeGestaoDeProduto.Todos, 2)).ReturnsAsync(pagina);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        var resultado = await _controller.Index(FiltroDeGestaoDeProduto.Todos, 2);

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_ResultadoDaGestao", parcial.ViewName);
        Assert.IsType<GestaoDeProdutosViewModel>(parcial.Model);
    }

    [Fact]
    public async Task Dado_ProdutoExistente_Quando_EdicaoGet_Entao_DeveRetornarPartialDoFormularioComODto()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto);
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Edicao(dto.ProdutoId);

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_FormularioProduto", parcial.ViewName);
        Assert.Same(dto, parcial.Model);
    }

    [Fact]
    public async Task Dado_ModelStateInvalido_Quando_EdicaoPost_Entao_DeveRetornarPartialCom400SemChamarServico()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto.ComImagem("https://x/atual.jpg"));
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(new List<CategoriaDTO>());
        _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

        var resultado = await _controller.Edicao(dto, imagem: null);

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal(400, parcial.StatusCode);
        _produtoServiceMock.Verify(s => s.Atualizar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EdicaoSemArquivo_Quando_EdicaoPost_Entao_DeveAtualizarComAImagemAtualSemChamarArmazenamento()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto.ComImagem("https://x/atual.jpg"));
        ConfigurarTempData();

        var resultado = await _controller.Edicao(dto, imagem: null);

        Assert.IsType<RedirectToActionResult>(resultado);
        _produtoServiceMock.Verify(s => s.Atualizar(It.Is<ProdutoDTO>(d => d.ImagemUrl == "https://x/atual.jpg")), Times.Once);
        _armazenamentoMock.Verify(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ArquivoDeFormatoInvalido_Quando_EdicaoPost_Entao_DeveRetornarPartialCom400SemAtualizar()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto.ComImagem("https://x/atual.jpg"));
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Edicao(dto, CriarArquivo("documento.pdf", "application/pdf"));

        Assert.Equal(400, Assert.IsType<PartialViewResult>(resultado).StatusCode);
        _produtoServiceMock.Verify(s => s.Atualizar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_EnvioDaImagemFalhou_Quando_EdicaoPost_Entao_DeveRetornarPartialCom400SemAtualizar()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto.ComImagem("https://x/atual.jpg"));
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(new List<CategoriaDTO>());
        _armazenamentoMock.Setup(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaFalha("Não foi possível enviar a imagem agora."));

        var resultado = await _controller.Edicao(dto, CriarArquivo());

        Assert.Equal(400, Assert.IsType<PartialViewResult>(resultado).StatusCode);
        _produtoServiceMock.Verify(s => s.Atualizar(It.IsAny<ProdutoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Dado_ProdutoValido_Quando_EdicaoPost_Entao_DeveRedirecionarParaIndexComConfirmacao()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto.ComImagem("https://x/atual.jpg"));
        _armazenamentoMock.Setup(s => s.Enviar(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultadoDoEnvioDeImagemDTO.ParaSucesso("https://x/nova.jpg"));
        ConfigurarTempData();

        var resultado = await _controller.Edicao(dto, CriarArquivo());

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirecionamento.ActionName);
        _produtoServiceMock.Verify(s => s.Atualizar(It.Is<ProdutoDTO>(d => d.ImagemUrl == "https://x/nova.jpg")), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    [Fact]
    public async Task Dado_ProdutoExistente_Quando_Detalhe_Entao_DeveRetornarPartialDoDetalheComSubcategorias()
    {
        var dto = CriarDtoValido();
        _produtoServiceMock.Setup(s => s.BuscarProdutoPorId(dto.ProdutoId)).ReturnsAsync(dto);
        _categoriaServiceMock.Setup(s => s.ListarComSubcategorias()).ReturnsAsync(new List<CategoriaDTO>());

        var resultado = await _controller.Detalhe(dto.ProdutoId);

        var parcial = Assert.IsType<PartialViewResult>(resultado);
        Assert.Equal("_DetalheProduto", parcial.ViewName);
        Assert.Same(dto, parcial.Model);
        Assert.NotNull(parcial.ViewData["Subcategorias"]);
    }

    [Fact]
    public async Task Dado_FiltroEPagina_Quando_Inativar_Entao_DeveRedirecionarComOsMesmosValores()
    {
        var id = Guid.NewGuid();
        ConfigurarTempData();

        var resultado = await _controller.Inativar(id, FiltroDeGestaoDeProduto.Todos, 3);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirecionamento.ActionName);
        Assert.Equal(FiltroDeGestaoDeProduto.Todos, redirecionamento.RouteValues!["filtro"]);
        Assert.Equal(3, redirecionamento.RouteValues["pagina"]);
        _produtoServiceMock.Verify(s => s.Inativar(id), Times.Once);
        Assert.NotNull(_controller.TempData["Confirmacao"]);
    }

    private static ProdutoDTO CriarDtoValido() => new()
    {
        ProdutoId = Guid.NewGuid(),
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
