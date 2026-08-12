using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class FavoritoTests
{
    [Fact]
    public void Dado_DadosValidos_Quando_CriarFavorito_Entao_DeveRetornarFavoritoInstanciado()
    {
        var produtoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var favorito = new Favorito(produtoId, usuarioId);

        Assert.Equal(produtoId, favorito.ProdutoId);
        Assert.Equal(usuarioId, favorito.UsuarioId);
    }

    [Fact]
    public void Dado_ProdutoInvalido_Quando_CriarFavorito_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Favorito(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarFavorito_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Favorito(Guid.NewGuid(), Guid.Empty));
    }
}
