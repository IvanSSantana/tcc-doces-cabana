using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public record ResumoDeBannerDTO(
    Guid BannerId,
    string ImagemUrl,
    string TextoAlternativo,
    string Destino,
    short Ordem,
    DateOnly DataInicio,
    DateOnly? DataFim,
    SituacaoDoBanner Situacao,
    bool Ativo);
