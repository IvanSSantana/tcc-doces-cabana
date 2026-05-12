using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

public class Produto
{
    public Guid ProdutoId { get; private set; }

    public Guid SubcategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

    public decimal Preco { get; private set; }

    // Status do produto usando ENUM
    // ProdutoStatus.Ativo
    // ProdutoStatus.Inativo
    // ProdutoStatus.ForaDeEstoque
    public ProdutoStatus Status { get; private set; }

    // Promoção é opcional
    // Por isso o 'Guid?' pode ter valor ou não (nullable)
    public Guid? PromocaoId { get; private set; }

    public string ImagemUrl { get; private set; } = default!;

    // Propriedades auxiliares para deixar o código mais legível
    public bool EstaAtivo => Status == ProdutoStatus.Ativo;

    public bool EstaInativo => Status == ProdutoStatus.Inativo;

    public bool EstaForaDeEstoque => Status == ProdutoStatus.ForaDeEstoque;

    // Construtor protegido para o Entity Framework
    protected Produto() { }

    // Construtor principal da entidade
    public Produto(Guid subcategoriaId, string nome, decimal preco, string imagemUrl, Guid id = default)
    {
        // Se nenhum ID for enviado
        // gera um novo automaticamente
        ProdutoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        // Todo produto nasce ativo
        Status = ProdutoStatus.Ativo;

        // Validações
        ValidarSubcategoria(subcategoriaId);
        ValidarNome(nome);
        ValidarPreco(preco);
        ValidarImagem(imagemUrl);

        // Atribuição dos valores
        SubcategoriaId = subcategoriaId;
        Nome = nome;
        Preco = preco;
        ImagemUrl = imagemUrl;
    }

    public void AlterarNome(string nome)
    {
        ValidarNome(nome);

        Nome = nome;  // [propriedade] = [parâmetro]
    }

    public void AlterarSubcategoriaId(Guid subcategoriaId)
    {
        ValidarSubcategoria(subcategoriaId);

        SubcategoriaId = subcategoriaId;
    }

    public void AlterarPreco(decimal preco)
    {
        ValidarPreco(preco);

        Preco = preco;
    }

    public void AlterarImagem(string url)
    {
        ValidarImagem(url);

        ImagemUrl = url;
    }

    // =========================
    // CONTROLE DE STATUS
    // =========================

    // Ativa o produto
    public void Ativar()
    {
        Status = ProdutoStatus.Ativo;
    }

    // Inativa o produto
    public void Inativar()
    {
        Status = ProdutoStatus.Inativo;
    }

    // Define produto sem estoque
    public void DefinirForaDeEstoque()
    {
        Status = ProdutoStatus.ForaDeEstoque;
    }

    // =========================
    // PROMOÇÃO
    // =========================

    public void AplicarPromocao(Guid promocaoId)
    {
        // Não permite Guid vazio
        if (promocaoId == Guid.Empty)
            throw new ArgumentException("Promoção inválida.", nameof(promocaoId));

        // Produto inativo não pode entrar em promoção
        if (EstaInativo)
            throw new InvalidOperationException("Produto inativo não pode entrar em promoção.");

        // Produto sem estoque também não entra em promoção
        if (EstaForaDeEstoque)
            throw new InvalidOperationException("Produto fora de estoque não pode entrar em promoção.");

        PromocaoId = promocaoId;
    }

    // Remove promoção
    public void RemoverPromocao()
    {
        PromocaoId = null;
    }

    // =========================
    // VALIDAÇÕES PRIVADAS
    // =========================

    private void ValidarSubcategoria(Guid subcategoriaId)
    {
        // Guid.Empty significa GUID inválido/vazio
        if (subcategoriaId == Guid.Empty)
            throw new ArgumentException("Subcategoria inválida.", nameof(subcategoriaId));
    }

    private void ValidarNome(string nome)
    {
        // Verifica se está vazio, null ou só espaços
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        // Nome mínimo de 3 caracteres
        if (nome.Length < 3)
            throw new ArgumentException("Nome deve ter no mínimo 3 caracteres.", nameof(nome));
    }

    private void ValidarPreco(decimal preco)
    {
        // Não permite preço zero ou negativo
        if (preco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.", nameof(preco));
    }

    private void ValidarImagem(string url)
    {
        // Verifica se a URL está vazia
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url), "Imagem é obrigatória!");

        // Verifica se a URL é válida
        // e se começa com http ou https
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL da imagem inválida.", nameof(url));
        }
    }
}