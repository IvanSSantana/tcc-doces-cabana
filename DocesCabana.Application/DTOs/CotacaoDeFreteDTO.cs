namespace DocesCabana.Application.DTOs;

public record CotacaoDeFreteDTO(
    string? CepConsultado,
    IReadOnlyList<OpcaoDeFreteDTO> Opcoes,
    string? Mensagem);
