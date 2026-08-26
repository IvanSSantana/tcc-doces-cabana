using DocesCabana.Domain.Entities;
using DocesCabana.MVC.Helpers;

namespace DocesCabana.Tests.Units.Helpers;

// GerarProdutosMock é internal (InternalsVisibleTo em DocesCabana.MVC.csproj),
// mesmo padrão de GerarAvaliacoesMock/GeradorDeAvaliacoesTests — função pura,
// sem DbContext, testável como unidade mesmo vivendo em DbInitializer.
public class DbInitializerProdutosTests
{
    // Monta o mesmo formato que DbInitializer.Semear monta a partir do banco,
    // reaproveitando a Taxonomia real em vez de duplicar nomes à parte.
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

    // RF-03/CA-03 (spec 020): prova que os cem produtos gerados nascem com
    // as quatro medidas > 0 — sem isso, o construtor de Produto (RN-01)
    // teria recusado a semeadura inteira, e é isso que este teste garante
    // que nunca acontece silenciosamente.
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

    // CA-09/plano §5: é o par que faz o peso cubado da transportadora
    // divergir do peso real — Adega pesada e compacta, Souvenir leve e
    // volumosa. Sem essa diferença por categoria, nenhum critério sobre
    // volume seria satisfazível.
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
