using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class SubcategoriaServiceTests
{
    private readonly Mock<ISubcategoriaRepository> _subcategoriaRepositoryMock;
    private readonly SubcategoriaService _subcategoriaService;

    public SubcategoriaServiceTests()
    {
        _subcategoriaRepositoryMock = new Mock<ISubcategoriaRepository>();
        _subcategoriaService = new SubcategoriaService(_subcategoriaRepositoryMock.Object);
    }

    [Fact]
    public async Task Dado_SubcategoriasCadastradas_Quando_BuscarTodasSubcategorias_Entao_DeveRetornarListaDeSubcategorias()
    {
        var categoriaId = Guid.NewGuid();
        var subcategorias = new List<Subcategoria>
        {
            new Subcategoria(categoriaId, "Doces de Tacho"),
            new Subcategoria(categoriaId, "Doces Caseiros")
        };

        _subcategoriaRepositoryMock.Setup(r => r.BuscarTodos())
            .ReturnsAsync(subcategorias);

        var resultado = await _subcategoriaService.BuscarTodasSubcategorias();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, s => s.Nome == "Doces de Tacho");
        Assert.Contains(resultado, s => s.Nome == "Doces Caseiros");
    }

    [Fact]
    public async Task Dado_NenhumaSubcategoriaCadastrada_Quando_BuscarTodasSubcategorias_Entao_DeveRetornarListaVazia()
    {
        _subcategoriaRepositoryMock.Setup(r => r.BuscarTodos())
            .ReturnsAsync(new List<Subcategoria>());

        var resultado = await _subcategoriaService.BuscarTodasSubcategorias();

        Assert.Empty(resultado);
    }
}
