using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class BannerTests
{
    private static readonly DateOnly Inicio = new(2026, 12, 1);
    private static readonly DateOnly Fim = new(2026, 12, 25);
    private const string ImagemValida = "https://exemplo.com/banner.png";

    private static Banner Criar(
        Guid? categoriaId = null, Guid? subcategoriaId = null, Guid? produtoId = null,
        string imagemUrl = ImagemValida, string textoAlternativo = "Brigadeiros de Natal",
        short ordem = 1, DateOnly? dataFim = null, bool semDestino = false)
    {
        if (!semDestino && categoriaId is null && subcategoriaId is null && produtoId is null)
            categoriaId = Guid.NewGuid();

        return new Banner(imagemUrl, textoAlternativo, ordem, Inicio, dataFim ?? Fim, categoriaId, subcategoriaId, produtoId);
    }

    [Fact]
    public void Dado_DadosValidos_Quando_CriarBanner_Entao_DeveNascerAtivoComOsDados()
    {
        var categoriaId = Guid.NewGuid();

        var banner = Criar(categoriaId: categoriaId);

        Assert.NotEqual(Guid.Empty, banner.BannerId);
        Assert.Equal(ImagemValida, banner.ImagemUrl);
        Assert.Equal("Brigadeiros de Natal", banner.TextoAlternativo);
        Assert.Equal(1, banner.Ordem);
        Assert.Equal(Inicio, banner.DataInicio);
        Assert.Equal(Fim, banner.DataFim);
        Assert.True(banner.Ativo);
        Assert.Equal(categoriaId, banner.CategoriaId);
        Assert.Equal(TipoDeDestinoDoBanner.Categoria, banner.TipoDeDestino);
    }

    [Fact]
    public void Dado_DestinoAusente_Quando_CriarBanner_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Criar(semDestino: true));
    }

    [Fact]
    public void Dado_DoisDestinos_Quando_CriarBanner_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Criar(categoriaId: Guid.NewGuid(), produtoId: Guid.NewGuid()));
    }

    [Fact]
    public void Dado_DestinoComGuidVazio_Quando_CriarBanner_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Criar(subcategoriaId: Guid.Empty));
    }

    [Fact]
    public void Dado_DestinoSubcategoria_Quando_CriarBanner_Entao_TipoDeveSerSubcategoria()
    {
        var banner = Criar(subcategoriaId: Guid.NewGuid());

        Assert.Equal(TipoDeDestinoDoBanner.Subcategoria, banner.TipoDeDestino);
    }

    [Fact]
    public void Dado_DestinoProduto_Quando_CriarBanner_Entao_TipoDeveSerProduto()
    {
        var banner = Criar(produtoId: Guid.NewGuid());

        Assert.Equal(TipoDeDestinoDoBanner.Produto, banner.TipoDeDestino);
    }

    [Fact]
    public void Dado_BannerDeCategoria_Quando_AlterarDestinoParaProduto_Entao_DeveLimparACategoria()
    {
        var banner = Criar(categoriaId: Guid.NewGuid());
        var produtoId = Guid.NewGuid();

        banner.AlterarDestino(null, null, produtoId);

        Assert.Null(banner.CategoriaId);
        Assert.Equal(produtoId, banner.ProdutoId);
        Assert.Equal(TipoDeDestinoDoBanner.Produto, banner.TipoDeDestino);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Dado_ImagemVazia_Quando_CriarBanner_Entao_DeveLancarArgumentException(string imagem)
    {
        Assert.ThrowsAny<ArgumentException>(() => Criar(imagemUrl: imagem));
    }

    [Fact]
    public void Dado_ImagemComMaisDe255Caracteres_Quando_AlterarImagem_Entao_DeveLancarArgumentException()
    {
        var banner = Criar();

        Assert.Throws<ArgumentException>(() => banner.AlterarImagem("https://exemplo.com/" + new string('a', 255)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Dado_TextoAlternativoVazio_Quando_CriarBanner_Entao_DeveLancarArgumentException(string texto)
    {
        Assert.ThrowsAny<ArgumentException>(() => Criar(textoAlternativo: texto));
    }

    [Fact]
    public void Dado_TextoAlternativoComMaisDe150Caracteres_Quando_CriarBanner_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Criar(textoAlternativo: new string('a', 151)));
    }

    [Fact]
    public void Dado_OrdemNegativa_Quando_CriarBanner_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Criar(ordem: -1));
    }

    [Fact]
    public void Dado_DataFimAnteriorAoInicio_Quando_AlterarPeriodo_Entao_DeveLancarArgumentException()
    {
        var banner = Criar();

        Assert.Throws<ArgumentException>(() => banner.AlterarPeriodo(Inicio, Inicio.AddDays(-1)));
    }

    [Fact]
    public void Dado_DataFimNula_Quando_EstaVigente_Entao_DeveSerVerdadeiroDepoisDoInicio()
    {
        var banner = Criar();
        banner.AlterarPeriodo(Inicio, null);

        Assert.True(banner.EstaVigente(Inicio.AddYears(3)));
        Assert.False(banner.EstaVigente(Inicio.AddDays(-1)));
    }

    [Fact]
    public void Dado_PeriodoFechado_Quando_EstaVigente_Entao_DeveRespeitarInicioEFimInclusivos()
    {
        var banner = Criar();

        Assert.True(banner.EstaVigente(Inicio));
        Assert.True(banner.EstaVigente(Fim));
        Assert.False(banner.EstaVigente(Fim.AddDays(1)));
    }

    [Fact]
    public void Dado_BannerDesativado_Quando_EstaVigente_Entao_DeveSerFalso()
    {
        var banner = Criar();

        banner.Desativar();

        Assert.False(banner.Ativo);
        Assert.False(banner.EstaVigente(Inicio));
    }

    [Fact]
    public void Dado_BannerDesativado_Quando_Ativar_Entao_DeveVoltarAoCarrossel()
    {
        var banner = Criar();
        banner.Desativar();

        banner.Ativar();

        Assert.True(banner.EstaVigente(Inicio));
    }

    [Fact]
    public void Dado_DadosNovos_Quando_AlterarApresentacao_Entao_DeveTrocarTextoEOrdem()
    {
        var banner = Criar();

        banner.AlterarApresentacao("Bolos de pote", 5);

        Assert.Equal("Bolos de pote", banner.TextoAlternativo);
        Assert.Equal(5, banner.Ordem);
    }
}
