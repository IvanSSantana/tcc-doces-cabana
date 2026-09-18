using DocesCabana.Domain.Enums;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public class Produto
{
    public Guid ProdutoId { get; private set; }

    public Guid SubcategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

    public string NomeNormalizado { get; private set; } = default!;

    public decimal Preco { get; private set; }

    public ProdutoStatus Status { get; private set; }

    public Guid? PromocaoId { get; private set; }

    public string ImagemUrl { get; private set; } = default!;

    public string? Descricao { get; private set; }

    public bool SemAcucar { get; private set; }

    public decimal Peso { get; private set; }         

    public decimal Altura { get; private set; }        

    public decimal Largura { get; private set; }       

    public decimal Comprimento { get; private set; }   

    public Subcategoria? Subcategoria { get; private set; }

    public Promocao? Promocao { get; private set; }

    protected Produto() { }

    public Produto(
        Guid subcategoriaId,
        string nome,
        decimal preco,
        string imagemUrl,
        decimal peso,
        decimal altura,
        decimal largura,
        decimal comprimento,
        ProdutoStatus status = ProdutoStatus.Ativo,
        Guid id = default,
        string? descricao = null,
        bool semAcucar = false)
    {
        ValidarSubcategoria(subcategoriaId);
        ValidarNome(nome);
        ValidarPreco(preco);
        ValidarImagem(imagemUrl);
        ValidarDescricao(descricao);
        ValidarDimensoes(peso, altura, largura, comprimento);

        ProdutoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        Status = status;
        SubcategoriaId = subcategoriaId;
        Nome = nome;
        NomeNormalizado = TextoHelper.Normalizar(nome);
        Preco = preco;
        ImagemUrl = imagemUrl;
        Descricao = descricao;
        SemAcucar = semAcucar;
        Peso = peso;
        Altura = altura;
        Largura = largura;
        Comprimento = comprimento;
    }

    public void AlterarDimensoes(decimal peso, decimal altura, decimal largura, decimal comprimento)
    {
        ValidarDimensoes(peso, altura, largura, comprimento);

        Peso = peso;
        Altura = altura;
        Largura = largura;
        Comprimento = comprimento;
    }

    public void AlterarNome(string nome)
    {
        ValidarNome(nome);

        Nome = nome;
        NomeNormalizado = TextoHelper.Normalizar(nome);
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

    public void AlterarDescricao(string? descricao)
    {
        ValidarDescricao(descricao);

        Descricao = descricao;
    }

    public void AlterarStatus(ProdutoStatus novoStatus) => Status = novoStatus;

    public bool DisponivelParaCompra() => Status == ProdutoStatus.Ativo;

    public void MarcarComoSemAcucar() => SemAcucar = true;

    public void DesmarcarSemAcucar() => SemAcucar = false;

    public void AplicarPromocao(Guid promocaoId)
    {
        if (promocaoId == Guid.Empty)
            throw new ArgumentException("Promoção inválida.", nameof(promocaoId));

        if (Status == ProdutoStatus.Inativo)
            throw new InvalidOperationException("Produto inativo não pode entrar em promoção.");

        if (Status == ProdutoStatus.ForaDeEstoque)
            throw new InvalidOperationException("Produto fora de estoque não pode entrar em promoção.");

        PromocaoId = promocaoId;
    }

    public void RemoverPromocao() => PromocaoId = null;

    private void ValidarSubcategoria(Guid subcategoriaId)
    {
        if (subcategoriaId == Guid.Empty)
            throw new ArgumentException("Subcategoria inválida.", nameof(subcategoriaId));
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        if (nome.Length < 3)
            throw new ArgumentException("Nome deve ter no mínimo 3 caracteres.", nameof(nome));
    }

    private void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.", nameof(preco));
    }

    private static void ValidarDescricao(string? descricao)
    {
        if (descricao is not null && descricao.Length > 4000)
            throw new ArgumentException("Descrição deve ter no máximo 4000 caracteres.", nameof(descricao));
    }

    private static void ValidarDimensoes(decimal peso, decimal altura, decimal largura, decimal comprimento)
    {
        if (peso <= 0)
            throw new ArgumentException("Peso deve ser maior que zero.", nameof(peso));

        if (altura <= 0)
            throw new ArgumentException("Altura deve ser maior que zero.", nameof(altura));

        if (largura <= 0)
            throw new ArgumentException("Largura deve ser maior que zero.", nameof(largura));

        if (comprimento <= 0)
            throw new ArgumentException("Comprimento deve ser maior que zero.", nameof(comprimento));
    }

    private void ValidarImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url), "Imagem é obrigatória!");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL da imagem inválida.", nameof(url));
        }
    }
}