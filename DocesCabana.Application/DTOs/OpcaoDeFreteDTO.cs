namespace DocesCabana.Application.DTOs;

// Uma opção de entrega devolvida pelo serviço de cotação (spec 020). ServicoId
// é o identificador do serviço na transportadora — a spec de fechamento (022)
// usa para casar a re-cotação com a opção escolhida.
public record OpcaoDeFreteDTO(
    int ServicoId,
    string Transportadora,
    string Servico,
    decimal Preco,
    int PrazoMinimoEmDias,
    int PrazoMaximoEmDias);
