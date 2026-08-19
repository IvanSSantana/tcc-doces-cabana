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
}
