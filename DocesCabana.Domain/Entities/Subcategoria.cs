namespace DocesCabana.Domain.Entities;

public class Subcategoria
{
    public Guid SubcategoriaId { get; private set; }

    public Guid CategoriaId { get; private set; }

    public string Nome { get; private set; } = default!;

    // Navegação filho -> pai. Anulável: vem null a menos que a consulta peça
    // Include. Ambas as pontas vivem no domínio, então é navegação normal
    // (RQ-10 da spec 003), diferente da referência a Usuario.
    public Categoria? Categoria { get; private set; }

    protected Subcategoria() { }

    public Subcategoria(Guid categoriaId, string nome, Guid id = default)
    {
        ValidarCategoria(categoriaId);
        ValidarNome(nome);

        SubcategoriaId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        CategoriaId = categoriaId;
        Nome = nome;
    }

    public void AlterarNome(string nome)
    {
        ValidarNome(nome);

        Nome = nome;
    }

    private void ValidarCategoria(Guid categoriaId)
    {
        if (categoriaId == Guid.Empty)
            throw new ArgumentException("Categoria inválida.", nameof(categoriaId));
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        if (nome.Length < 3 || nome.Length > 100)
            throw new ArgumentException("Nome deve ter entre 3 e 100 caracteres.", nameof(nome));
    }
}
