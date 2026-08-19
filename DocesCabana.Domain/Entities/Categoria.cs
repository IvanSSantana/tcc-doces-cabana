namespace DocesCabana.Domain.Entities;

public class Categoria
{
    public Guid CategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

    // Navegação pai -> filhos, só leitura por fora (spec 012): quem cria uma
    // subcategoria é o construtor de Subcategoria, não este agregado. Mesmo
    // padrão de coleção rastreada pelo EF Core via campo privado que
    // Avaliacao.Votos já usa desde a spec 008.
    private readonly List<Subcategoria> _subcategorias = [];

    public IReadOnlyCollection<Subcategoria> Subcategorias => _subcategorias.AsReadOnly();

    protected Categoria() { }

    public Categoria(string nome, Guid id = default)
    {
        ValidarNome(nome);

        CategoriaId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        Nome = nome;
    }

    public void AlterarNome(string nome)
    {
        ValidarNome(nome);

        Nome = nome;
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        if (nome.Length < 3 || nome.Length > 100)
            throw new ArgumentException("Nome deve ter entre 3 e 100 caracteres.", nameof(nome));
    }
}
