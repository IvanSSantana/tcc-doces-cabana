namespace DocesCabana.Application.DTOs;

// Só o CEP — o que a barreira de entrada (RF-09 da spec 020) precisa
// validar antes de qualquer consulta ao serviço de entrega.
public record ConsultaDeFreteDTO(string Cep);
