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

    // spec 023, RF-02: a linha da lista — resume, não aprofunda.
    public static ResumoDePedidoDTO ToResumoDTO(Pedido pedido) => new(
        pedido.PedidoId,
        pedido.NumeroVisivel(),
        pedido.Data,
        pedido.Status,
        pedido.Itens.Sum(i => (int)i.Quantidade),
        pedido.Valor);

    // spec 023, RF-06 a RF-10: o pedido inteiro, como estava no fechamento
    // (RN-02) — preço, frete, transportadora e prazo vêm gravados, nunca
    // recalculados.
    public static DetalheDePedidoDTO ToDetalheDTO(Pedido pedido, Pagamento? pagamento) => new()
    {
        PedidoId = pedido.PedidoId,
        Numero = pedido.NumeroVisivel(),
        Data = pedido.Data,
        Status = pedido.Status,
        Itens = pedido.Itens
            .Select(i => new ItemDoDetalheDePedidoDTO(
                i.Produto?.Nome ?? string.Empty,
                i.Produto?.ImagemUrl ?? string.Empty,
                i.Quantidade,
                i.PrecoUnitario,
                i.PrecoUnitario * i.Quantidade))
            .ToList(),
        Endereco = pedido.EnderecoEntrega is not null ? EnderecoMapper.ToDTO(pedido.EnderecoEntrega) : new EnderecoDTO(),
        Transportadora = pedido.Transportadora,
        Servico = pedido.Servico,
        PrazoMinimoEmDias = pedido.PrazoMinimoEmDias,
        PrazoMaximoEmDias = pedido.PrazoMaximoEmDias,
        ValorDosProdutos = pedido.Valor - pedido.ValorDoFrete,
        ValorDoFrete = pedido.ValorDoFrete,
        MetodoPagamento = pagamento?.Metodo ?? default,
        StatusDoPagamento = pagamento?.Status ?? PagamentoStatus.Pendente
    };
}
