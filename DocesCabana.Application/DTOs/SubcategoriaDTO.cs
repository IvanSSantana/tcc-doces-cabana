namespace DocesCabana.Application.DTOs;

public class SubcategoriaDTO
{
    public Guid SubcategoriaId { get; init; }

    public string Nome { get; init; } = string.Empty;

    // Derivado do nome, como o da categoria (Apelido.De) — único DENTRO da
    // categoria dona, não na loja inteira (spec 016, RN-03).
    public string Apelido { get; init; } = string.Empty;
}
