namespace DocesCabana.Domain.Entities;

public class Produto
{
    public Guid ProdutoId { get; private set; }
    public Guid SubcategoriaId { get; private set; }
    public string Nome { get; private set; } = default!;
    public decimal Preco { get; private set; }
    public int Status { get; private set; }
    public Guid? PromocaoId { get; private set; }
    public string ImagemUrl { get; private set; } = default!;

    protected Produto() { }

    public Produto(Guid subcategoriaId, string nome, decimal preco, string imagemUrl, Guid id = default)
    {
        if (id == default)
            id = Guid.NewGuid();

        ProdutoId = id;
        Status = 1;

        ValidarSubcategoria(subcategoriaId);
        ValidarNome(nome);
        ValidarPreco(preco);
        ValidarImagem(imagemUrl);

        SubcategoriaId = subcategoriaId;
        Nome = nome;
        Preco = preco;
        ImagemUrl = imagemUrl;
    }

    private void ValidarSubcategoria(Guid subcategoriaId)
    {
        if (subcategoriaId == Guid.Empty)
            throw new ArgumentException("Subcategoria inválida.");
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException("Nome é obrigatório!");

        if (nome.Length < 3)
            throw new ArgumentException("Nome deve ter no mínimo 3 caracteres.");
    }

    private void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.");
    }

    private void ValidarImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException("Imagem é obrigatória!");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("URL da imagem inválida.");
    }

    public void AlterarNome(string nome)
    {
        ValidarNome(nome);
        Nome = nome;
    }

    public void AlterarPreco(decimal preco)
    {
        ValidarPreco(preco);
        Preco = preco;
    }

    public void AlterarSubcategoria(Guid subcategoriaId)
    {
        ValidarSubcategoria(subcategoriaId);
        SubcategoriaId = subcategoriaId;
    }

    public void AlterarImagem(string url)
    {
        ValidarImagem(url);
        ImagemUrl = url;
    }

    public void Ativar() => Status = 1;

    public void Inativar() => Status = 0;

    public void AplicarPromocao(Guid promocaoId)
    {
        if (promocaoId == Guid.Empty)
            throw new ArgumentException("Promoção inválida.");

        if (Status == 0)
            throw new InvalidOperationException("Produto inativo não pode entrar em promoção.");

        PromocaoId = promocaoId;
    }

    public void RemoverPromocao() => PromocaoId = null;
}