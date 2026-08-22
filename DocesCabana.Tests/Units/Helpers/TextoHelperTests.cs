using DocesCabana.Domain.Helpers;

namespace DocesCabana.Tests.Units.Helpers;

public class TextoHelperTests
{
    // Nomes reais da loja (DbInitializer) — os que motivaram a coluna
    // normalizada (spec 016, plano §1): sem a normalização, buscar "cafe"
    // não encontra "Café" no SQLite (Contains vira instr, sensível a caixa
    // e a acento).
    [Theory]
    [InlineData("Café", "cafe")]
    [InlineData("Cachaça", "cachaca")]
    [InlineData("Empório", "emporio")]
    [InlineData("Pelúcia", "pelucia")]
    [InlineData("Brigadeiro", "brigadeiro")]
    [InlineData("  Espaço nas pontas  ", "espaco nas pontas")]
    [InlineData("CAIXA ALTA", "caixa alta")]
    public void Dado_UmTexto_Quando_Normalizar_Entao_DeveSairSemAcentoEEmCaixaBaixa(string texto, string esperado)
    {
        var resultado = TextoHelper.Normalizar(texto);

        Assert.Equal(esperado, resultado);
    }
}
