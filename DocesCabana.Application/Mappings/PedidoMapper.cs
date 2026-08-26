using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class PedidoMapper
{
    public static ConfirmacaoDePedidoDTO ToConfirmacaoDTO(Pedido pedido, MetodoPagamento metodoPagamento) => new()
    {
        PedidoId = pedido.PedidoId,
        Numero = pedido.NumeroVisivel(),
        Itens = pedido.Itens
            .Select(i => new ItemDaConfirmacaoDTO(
                i.Produto?.Nome ?? string.Empty,
                i.Produto?.ImagemUrl ?? string.Empty,
                i.Quantidade,
                i.PrecoUnitario,
                i.PrecoUnitario * i.Quantidade))
            .ToList(),
        // ValorDoFrete é gravado à parte (RN-01); ValorDosProdutos é a
        // diferença — mesma razão de Pedido.ValorDoFrete não ser derivado
        // de Valor (plano §6), aqui é o caminho inverso, sem terceiro
        // valor gravado para não duplicar a fonte da verdade.
        ValorDosProdutos = pedido.Valor - pedido.ValorDoFrete,
        ValorDoFrete = pedido.ValorDoFrete,
        Transportadora = pedido.Transportadora,
        Servico = pedido.Servico,
        PrazoMinimoEmDias = pedido.PrazoMinimoEmDias,
        PrazoMaximoEmDias = pedido.PrazoMaximoEmDias,
        MetodoPagamento = metodoPagamento
    };
}
