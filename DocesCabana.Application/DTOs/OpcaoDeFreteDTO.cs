namespace DocesCabana.Application.DTOs;

public record OpcaoDeFreteDTO(
    int ServicoId,
    string Transportadora,
    string Servico,
    decimal Preco,
    int PrazoMinimoEmDias,
    int PrazoMaximoEmDias);
