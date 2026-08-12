using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class CategoriaTests
{
    [Fact]
    public void Dado_NomeValido_Quando_CriarCategoria_Entao_DeveRetornarCategoriaInstanciada()
    {
        var categoria = new Categoria("Doces");

        Assert.NotNull(categoria);
        Assert.Equal("Doces", categoria.Nome);
        Assert.NotEqual(Guid.Empty, categoria.CategoriaId);
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("Do", typeof(ArgumentException))]
    public void Dado_NomeInvalido_Quando_CriarCategoria_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () => new Categoria(nome!));
    }

    [Fact]
    public void Dado_NomeComCentoEUmCaracteres_Quando_CriarCategoria_Entao_DeveLancarArgumentException()
    {
        var nome = new string('a', 101);

        Assert.Throws<ArgumentException>(() => new Categoria(nome));
    }

    [Fact]
    public void Dado_NovoNomeValido_Quando_AlterarNome_Entao_DeveAtualizarNome()
    {
        var categoria = new Categoria("Doces");

        categoria.AlterarNome("Salgados");

        Assert.Equal("Salgados", categoria.Nome);
    }
}
