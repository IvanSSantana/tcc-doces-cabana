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
                i.CalcularValorDaLinha(),
                i.DescontoDaPromocao,
                i.RotuloDaPromocao))
            .ToList(),

        ValorDosProdutos = pedido.ValorDosProdutos(),
        ValorDoFrete = pedido.ValorDoFrete,
        CodigoDoCupom = pedido.CodigoDoCupom,
        DescontoDoCupom = pedido.DescontoDoCupom,
        DescontoNoFrete = pedido.DescontoNoFrete,
        Modalidade = pedido.Modalidade,
        DataDaRetirada = pedido.DataDaRetirada,
        InicioDaRetirada = pedido.InicioDaRetirada,
        FimDaRetirada = pedido.FimDaRetirada,
        Transportadora = pedido.Transportadora,
        Servico = pedido.Servico,
        PrazoMinimoEmDias = pedido.PrazoMinimoEmDias,
        PrazoMaximoEmDias = pedido.PrazoMaximoEmDias,
        MetodoPagamento = metodoPagamento
    };

    public static ResumoDePedidoDTO ToResumoDTO(Pedido pedido) => new(
        pedido.PedidoId,
        pedido.NumeroVisivel(),
        pedido.Data,
        pedido.Status,
        pedido.Itens.Sum(i => (int)i.Quantidade),
        pedido.Valor,
        pedido.Status is PedidoStatus.Pendente or PedidoStatus.Confirmado or PedidoStatus.Enviado);

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
                i.CalcularValorDaLinha(),
                i.DescontoDaPromocao,
                i.RotuloDaPromocao))
            .ToList(),
        Endereco = pedido.EnderecoEntrega is not null ? EnderecoMapper.ToDTO(pedido.EnderecoEntrega) : new EnderecoDTO(),
        Modalidade = pedido.Modalidade,
        DataDaRetirada = pedido.DataDaRetirada,
        InicioDaRetirada = pedido.InicioDaRetirada,
        FimDaRetirada = pedido.FimDaRetirada,
        Transportadora = pedido.Transportadora,
        Servico = pedido.Servico,
        PrazoMinimoEmDias = pedido.PrazoMinimoEmDias,
        PrazoMaximoEmDias = pedido.PrazoMaximoEmDias,
        ValorDosProdutos = pedido.ValorDosProdutos(),
        ValorDoFrete = pedido.ValorDoFrete,
        CodigoDoCupom = pedido.CodigoDoCupom,
        DescontoDoCupom = pedido.DescontoDoCupom,
        DescontoNoFrete = pedido.DescontoNoFrete,
        MetodoPagamento = pagamento?.Metodo ?? default,
        StatusDoPagamento = pagamento?.Status ?? PagamentoStatus.Pendente
    };
}
