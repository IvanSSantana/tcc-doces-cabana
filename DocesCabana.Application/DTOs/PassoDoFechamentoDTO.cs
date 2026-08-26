using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

// O que a tela do carrinho precisa para renderizar o passo ativo do
// fechamento (spec 022, RF-01). Carrega mais do que qualquer passo sozinho
// usa porque `_PassosDoFechamento.cshtml` (o indicador) é comum aos quatro.
public class PassoDoFechamentoDTO
{
    public PassoDoFechamento PassoAtivo { get; init; }

    // RF-03: quem já está autenticado não vê o passo de conta.
    public IReadOnlyList<PassoDoFechamento> PassosVisiveis { get; init; } = [];

    public CarrinhoDTO Carrinho { get; init; } = new();

    // RF-06: o principal já vem marcado.
    public IReadOnlyList<EnderecoDTO> Enderecos { get; init; } = [];

    public Guid? EnderecoSelecionadoId { get; init; }

    // RF-08/RF-09: cotada para o endereço escolhido, não por CEP digitado —
    // diferente da caixa de CEP que a spec 020 pôs no resumo do carrinho.
    public CotacaoDeFreteDTO? Cotacao { get; init; }

    public int? ServicoDeEntregaSelecionadoId { get; init; }
}
