using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public record ResumoDePromocaoDTO(
    Guid PromocaoId,
    string Nome,
    string RotuloDoDesconto,
    DateOnly DataInicio,
    DateOnly DataFim,
    string Abrangencia,
    SituacaoDaPromocao Situacao,
    bool Ativa);
