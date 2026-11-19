using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class PromocaoDTO
{
    public Guid PromocaoId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public TipoDeDesconto Tipo { get; set; }

    public decimal? Valor { get; set; }

    public short? QuantidadeLeve { get; set; }

    public short? QuantidadePague { get; set; }

    public DateOnly? DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public AbrangenciaDaPromocao Abrangencia { get; set; }

    public Guid? CategoriaId { get; set; }

    public Guid? SubcategoriaId { get; set; }

    public List<Guid> ProdutoIds { get; set; } = [];
}
