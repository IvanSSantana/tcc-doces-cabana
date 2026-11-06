using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext.Seed;

namespace DocesCabana.Tests.Units.Seed;

public class DbInitializerProdutosTests
{
    private static Dictionary<string, Dictionary<string, Subcategoria>> MontarSubcategoriasPorCategoria()
    {
        var resultado = new Dictionary<string, Dictionary<string, Subcategoria>>();
        foreach (var (nomeCategoria, nomesSubcategorias) in DbInitializer.Taxonomia)
        {
            var categoriaId = Guid.NewGuid();
            resultado[nomeCategoria] = nomesSubcategorias
                .ToDictionary(nome => nome, nome => new Subcategoria(categoriaId, nome));
        }

        return resultado;
    }

    [Fact]
    public void Dado_ATaxonomiaReal_Quando_GerarProdutosMock_Entao_TodoProdutoDeveTerAsQuatroMedidasMaioresQueZero()
    {
        var subcategoriasPorCategoria = MontarSubcategoriasPorCategoria();

        var produtos = DbInitializer.GerarProdutosMock(subcategoriasPorCategoria);

        Assert.NotEmpty(produtos);
        Assert.All(produtos, p =>
        {
            Assert.True(p.Peso > 0, $"{p.Nome}: peso {p.Peso}");
            Assert.True(p.Altura > 0, $"{p.Nome}: altura {p.Altura}");
            Assert.True(p.Largura > 0, $"{p.Nome}: largura {p.Largura}");
            Assert.True(p.Comprimento > 0, $"{p.Nome}: comprimento {p.Comprimento}");
        });
    }

    [Fact]
    public void Dado_ATaxonomiaReal_Quando_GerarProdutosMock_Entao_AdegaDeveSerMaisPesadaQueSouvenirEMenosVolumosa()
    {
        var subcategoriasPorCategoria = MontarSubcategoriasPorCategoria();
        var produtos = DbInitializer.GerarProdutosMock(subcategoriasPorCategoria);

        var subcategoriasDeAdega = subcategoriasPorCategoria["Adega"].Values.Select(s => s.SubcategoriaId).ToHashSet();
        var subcategoriasDeSouvenir = subcategoriasPorCategoria["Souvenir"].Values.Select(s => s.SubcategoriaId).ToHashSet();

        var produtoAdega = produtos.First(p => subcategoriasDeAdega.Contains(p.SubcategoriaId));
        var produtoSouvenir = produtos.First(p => subcategoriasDeSouvenir.Contains(p.SubcategoriaId));

        Assert.True(produtoAdega.Peso > produtoSouvenir.Peso);

        var volumeAdega = produtoAdega.Altura * produtoAdega.Largura * produtoAdega.Comprimento;
        var volumeSouvenir = produtoSouvenir.Altura * produtoSouvenir.Largura * produtoSouvenir.Comprimento;
        Assert.True(volumeSouvenir > volumeAdega);
    }
}
