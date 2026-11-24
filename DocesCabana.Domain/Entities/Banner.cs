using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

/// <summary>
/// Imagem promocional do carrossel da página inicial. Todo banner leva a exatamente um lugar
/// do catálogo — categoria, subcategoria ou produto — guardado por chave, para que renomear
/// uma categoria não quebre o link.
/// </summary>
public class Banner
{
    public const int TamanhoMaximoDoTextoAlternativo = 150;
    public const int TamanhoMaximoDaImagem = 255;

    public Guid BannerId { get; private set; }

    public string ImagemUrl { get; private set; } = default!;

    public string TextoAlternativo { get; private set; } = default!;

    public short Ordem { get; private set; }

    public DateOnly DataInicio { get; private set; }

    public DateOnly? DataFim { get; private set; }

    public bool Ativo { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public Guid? SubcategoriaId { get; private set; }

    public Guid? ProdutoId { get; private set; }

    public Categoria? Categoria { get; private set; }

    public Subcategoria? Subcategoria { get; private set; }

    public Produto? Produto { get; private set; }

    public TipoDeDestinoDoBanner TipoDeDestino =>
        CategoriaId is not null ? TipoDeDestinoDoBanner.Categoria
        : SubcategoriaId is not null ? TipoDeDestinoDoBanner.Subcategoria
        : TipoDeDestinoDoBanner.Produto;

    protected Banner() { }

    public Banner(
        string imagemUrl,
        string textoAlternativo,
        short ordem,
        DateOnly dataInicio,
        DateOnly? dataFim,
        Guid? categoriaId,
        Guid? subcategoriaId,
        Guid? produtoId,
        Guid id = default)
    {
        BannerId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        AlterarImagem(imagemUrl);
        AlterarApresentacao(textoAlternativo, ordem);
        AlterarPeriodo(dataInicio, dataFim);
        AlterarDestino(categoriaId, subcategoriaId, produtoId);
        Ativo = true;
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public void AlterarImagem(string imagemUrl)
    {
        if (string.IsNullOrWhiteSpace(imagemUrl))
            throw new ArgumentNullException(nameof(imagemUrl), "Imagem é obrigatória!");

        if (imagemUrl.Length > TamanhoMaximoDaImagem)
            throw new ArgumentException($"URL da imagem deve ter no máximo {TamanhoMaximoDaImagem} caracteres.", nameof(imagemUrl));

        if (!Uri.TryCreate(imagemUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL da imagem inválida.", nameof(imagemUrl));
        }

        ImagemUrl = imagemUrl;
    }

    public void AlterarApresentacao(string textoAlternativo, short ordem)
    {
        if (string.IsNullOrWhiteSpace(textoAlternativo))
            throw new ArgumentNullException(nameof(textoAlternativo), "Texto alternativo é obrigatório!");

        if (textoAlternativo.Length > TamanhoMaximoDoTextoAlternativo)
            throw new ArgumentException(
                $"Texto alternativo deve ter no máximo {TamanhoMaximoDoTextoAlternativo} caracteres.", nameof(textoAlternativo));

        if (ordem < 0)
            throw new ArgumentException("Ordem não pode ser negativa.", nameof(ordem));

        TextoAlternativo = textoAlternativo.Trim();
        Ordem = ordem;
    }

    public void AlterarPeriodo(DateOnly dataInicio, DateOnly? dataFim)
    {
        if (dataFim < dataInicio)
            throw new ArgumentException("Data de fim não pode ser anterior à data de início.", nameof(dataFim));

        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public void AlterarDestino(Guid? categoriaId, Guid? subcategoriaId, Guid? produtoId)
    {
        if (categoriaId == Guid.Empty || subcategoriaId == Guid.Empty || produtoId == Guid.Empty)
            throw new ArgumentException("Destino do banner inválido.");

        var destinosEscolhidos =
            (categoriaId is not null ? 1 : 0) +
            (subcategoriaId is not null ? 1 : 0) +
            (produtoId is not null ? 1 : 0);

        if (destinosEscolhidos != 1)
            throw new ArgumentException("Escolha exatamente um destino para o banner: categoria, subcategoria ou produto.");

        CategoriaId = categoriaId;
        SubcategoriaId = subcategoriaId;
        ProdutoId = produtoId;
    }

    public bool EstaVigente(DateOnly hoje) =>
        Ativo && hoje >= DataInicio && (DataFim is null || hoje <= DataFim);
}
