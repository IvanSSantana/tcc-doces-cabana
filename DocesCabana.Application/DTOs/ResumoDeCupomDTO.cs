using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public record ResumoDeCupomDTO(
    Guid CupomId,
    string Codigo,
    string RotuloDoDesconto,
    DateOnly DataInicio,
    DateOnly DataFim,
    string Restricoes,
    int Usos,
    SituacaoDaPromocao Situacao,
    bool Ativo);
