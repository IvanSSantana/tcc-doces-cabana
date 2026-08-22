using DocesCabana.Domain.Enums;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public class Produto
{
    public Guid ProdutoId { get; private set; }

    public Guid SubcategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

    // Derivado de Nome nos dois únicos pontos que o alteram — construtor e
    // AlterarNome — nunca atribuído por fora (spec 016, RN-02). É o que a
    // busca por texto compara, sem acento e sem caixa: sem esta coluna,
    // "cafe" não encontraria "Café" no SQLite (Contains vira instr,
    // sensível aos dois).
    public string NomeNormalizado { get; private set; } = default!;

    public decimal Preco { get; private set; }

    public ProdutoStatus Status { get; private set; }

    public Guid? PromocaoId { get; private set; }

    public string ImagemUrl { get; private set; } = default!;

    // Opcional: produtos cadastrados antes desta feature ficam sem descrição
    // até serem recadastrados (spec 008, fora de escopo editar os antigos).
    public string? Descricao { get; private set; }

    // Característica do produto, não lugar na hierarquia (spec 012, RN-04):
    // "Doces Caseiros" e "Doces Zero" se fundiram em "Doces", e é esta
    // marcação que preserva a distinção que a fusão apagaria.
    public bool SemAcucar { get; private set; }

    // Navegações filho -> pai, anuláveis (vêm null sem Include). Ambas as
    // pontas vivem no domínio, então são navegação normal (RQ-10 da spec 003).
    public Subcategoria? Subcategoria { get; private set; }

    public Promocao? Promocao { get; private set; }

    protected Produto() { }

    public Produto(
        Guid subcategoriaId,
        string nome,
        decimal preco,
        string imagemUrl,
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