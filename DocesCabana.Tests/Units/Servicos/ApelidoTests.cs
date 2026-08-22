using DocesCabana.Application.Servicos;

namespace DocesCabana.Tests.Units.Servicos;

public class ApelidoTests
{
    [Theory]
    [InlineData("Empório", "emporio")]
    [InlineData("Doces", "doces")]
    [InlineData("Adega", "adega")]
    [InlineData("Souvenir", "souvenir")]
    [InlineData("Bolachas / Rosquinhas", "bolachas-rosquinhas")]
    public void Dado_NomeDeCategoria_Quando_GerarApelido_Entao_DeveSerLegivelSemAcentoEMinusculo(string nome, string apelidoEsperado)
    {
        var apelido = Apelido.De(nome);

        Assert.Equal(apelidoEsperado, apelido);
    }

    [Fact]
    public void Dado_AsQuatroCategoriasSemeadas_Quando_GerarApelidos_Entao_DevemSerDistintos()
    {
        var categorias = new[] { "Doces", "Empório", "Adega", "Souvenir" };

        var apelidos = categorias.Select(Apelido.De).ToList();

        Assert.Equal(apelidos.Count, apelidos.Distinct().Count());
    }

    // Taxonomia real (DbInitializer, spec 012 §11) — a mesma fonte que semeia
    // a base. "Cappuccino" existe em Doces e em Empório: a RN-03 (spec 016)
    // só exige unicidade DENTRO de cada categoria, não na loja inteira, e é
    // esse relaxamento que este teste prova.
    private static readonly (string Categoria, string[] Subcategorias)[] Taxonomia =
    [
        ("Doces", ["Barras", "Bolachas / Rosquinhas", "Box", "Combos", "Compotas", "Cappuccino", "Latas", "Palhas", "Potes", "Quindim", "Raspa de Tachos", "Sorvetes"]),
        ("Empório", ["Café", "Cappuccino", "Charcutaria", "Croissant", "Desidratados", "Geleias", "Manteiga", "Mel", "Molho", "Risotto"]),
        ("Adega", ["Cachaça", "Licor", "Licor Caseiro", "Vinhos"]),
        ("Souvenir", ["Bijuterias", "Canecas", "Chaveiros", "Kits", "Pelúcia"]),
    ];

    [Fact]
    public void Dado_ATaxonomiaReal_Quando_GerarApelidosDeSubcategoria_Entao_DevemSerDistintosDentroDeCadaCategoria()
    {
        foreach (var (categoria, subcategorias) in Taxonomia)
        {
            var apelidos = subcategorias.Select(Apelido.De).ToList();

            Assert.True(apelidos.Count == apelidos.Distinct().Count(),
                $"A categoria \"{categoria}\" tem apelidos de subcategoria colidindo: {string.Join(", ", apelidos)}");
        }
    }
}
