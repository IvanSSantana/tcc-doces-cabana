using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

// A linha da lista "Meus pedidos" (spec 023, RF-02) — resume, não aprofunda;
// o detalhe é tela própria (DetalheDePedidoDTO).
public record ResumoDePedidoDTO(
    Guid PedidoId,
    string Numero,
    DateTime Data,
    PedidoStatus Status,
    int QuantidadeDeItens,
    decimal ValorTotal);
