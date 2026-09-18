using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public record ResumoDePedidoDTO(
    Guid PedidoId,
    string Numero,
    DateTime Data,
    PedidoStatus Status,
    int QuantidadeDeItens,
    decimal ValorTotal);
