using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class SubcategoriaTests
{
    private readonly Guid _categoriaValida = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarSubcategoria_Entao_DeveRetornarSubcategoriaInstanciada()
    {
        var subcategoria = new Subcategoria(_categoriaValida, "Doces de Tacho");

        Assert.NotNull(subcategoria);
        Assert.Equal("Doces de Tacho", subcategoria.Nome);
        Assert.Equal(_categoriaValida, subcategoria.CategoriaId);
    }

    [Fact]
    public void Dado_CategoriaInvalida_Quando_CriarSubcategoria_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Subcategoria(Guid.Empty, "Doces de Tacho"));
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("Do", typeof(ArgumentException))]
    public void Dado_NomeInvalido_Quando_CriarSubcategoria_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () => new Subcategoria(_categoriaValida, nome!));
    }

    [Fact]
    public void Dado_NovoNomeValido_Quando_AlterarNome_Entao_DeveAtualizarNome()
    {
        var subcategoria = new Subcategoria(_categoriaValida, "Doces de Tacho");

        subcategoria.AlterarNome("Doces Caseiros");

        Assert.Equal("Doces Caseiros", subcategoria.Nome);
    }
}
