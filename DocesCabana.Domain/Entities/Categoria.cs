namespace DocesCabana.Domain.Entities;

public class Categoria
{
    public Guid CategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

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
