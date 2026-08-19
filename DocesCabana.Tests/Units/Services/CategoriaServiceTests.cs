using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class CategoriaServiceTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly CategoriaService _categoriaService;

    public CategoriaServiceTests()
    {
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _categoriaService = new CategoriaService(_categoriaRepositoryMock.Object, _produtoRepositoryMock.Object);
    }

    [Fact]
    public async Task Dado_SubcategoriasComContagensDiferentes_Quando_Listar_Entao_DeveOrdenarPelaComMaisProdutos()
    {
        var categoria = new Categoria("Doces");
        var poucos = new Subcategoria(categoria.CategoriaId, "Combos");
        var muitos = new Subcategoria(categoria.CategoriaId, "Barras");
        AdicionarSubcategorias(categoria, muitos, poucos);

        _categoriaRepositoryMock.Setup(r => r.BuscarTodasComSubcategorias()).ReturnsAsync([categoria]);
        _produtoRepositoryMock.Setup(r => r.ContarDisponivelPorSubcategoria()).ReturnsAsync(new Dictionary<Guid, int>
        {
            [poucos.SubcategoriaId] = 1,
            [muitos.SubcategoriaId] = 10,
        });

        var resultado = await _categoriaService.ListarComSubcategorias();

        Assert.Equal(["Barras", "Combos"], resultado[0].Subcategorias.Select(s => s.Nome));
    }

    [Fact]
    public async Task Dado_ApelidoConhecido_Quando_BuscarPorApelido_Entao_DeveEncontrarACategoria()
    {
        var categoria = new Categoria("Empório");
        _categoriaRepositoryMock.Setup(r => r.BuscarTodasComSubcategorias()).ReturnsAsync([categoria]);
        _produtoRepositoryMock.Setup(r => r.ContarDisponivelPorSubcategoria()).ReturnsAsync([]);

        var resultado = await _categoriaService.BuscarPorApelido("emporio");

        Assert.NotNull(resultado);
        Assert.Equal(categoria.CategoriaId, resultado!.CategoriaId);
    }

    [Fact]
    public async Task Dado_ApelidoDesconhecido_Quando_BuscarPorApelido_Entao_DeveRetornarNulo()
    {
        _categoriaRepositoryMock.Setup(r => r.BuscarTodasComSubcategorias()).ReturnsAsync([]);
        _produtoRepositoryMock.Setup(r => r.ContarDisponivelPorSubcategoria()).ReturnsAsync([]);

        var resultado = await _categoriaService.BuscarPorApelido("inexistente");

        Assert.Null(resultado);
    }

    private static void AdicionarSubcategorias(Categoria categoria, params Subcategoria[] subcategorias)
    {
        var campo = typeof(Categoria).GetField("_subcategorias", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var lista = (List<Subcategoria>)campo.GetValue(categoria)!;
        lista.AddRange(subcategorias);
    }
}
