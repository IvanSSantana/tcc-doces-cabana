using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class PassoDoFechamentoDTO
{
    public PassoDoFechamento PassoAtivo { get; init; }

    public IReadOnlyList<PassoDoFechamento> PassosVisiveis { get; init; } = [];

    public CarrinhoDTO Carrinho { get; init; } = new();

    public IReadOnlyList<EnderecoDTO> Enderecos { get; init; } = [];

    public Guid? EnderecoSelecionadoId { get; init; }

    public CotacaoDeFreteDTO? Cotacao { get; init; }

    public int? ServicoDeEntregaSelecionadoId { get; init; }
}
