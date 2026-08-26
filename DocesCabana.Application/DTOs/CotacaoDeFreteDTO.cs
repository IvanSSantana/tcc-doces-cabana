namespace DocesCabana.Application.DTOs;

// Resultado de uma cotação de frete (spec 020). Mensagem só é preenchida
// quando não foi possível cotar (serviço indisponível, CEP não atendido) —
// falha de serviço externo é condição esperada (RN-02 da spec 020, Princípio
// VIII), nunca vira exceção. Opcoes vem vazia nesse caso.
public record CotacaoDeFreteDTO(
    string? CepConsultado,
    IReadOnlyList<OpcaoDeFreteDTO> Opcoes,
    string? Mensagem);
