using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

// Uma linha já resolvida para a tela — preço vem sempre do produto atual
// (RN-04), nunca de coluna própria.
public class LinhaDoCarrinhoDTO
{
    public Guid ProdutoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string ImagemUrl { get; init; } = string.Empty;

    public decimal PrecoUnitario { get; init; }

    public short Quantidade { get; init; }

    public decimal ValorDaLinha { get; init; }

    // RN-06: Nenhum, ForaDoCatalogo ou ForaDeEstoque — só a mensagem muda.
    public MotivoIndisponibilidade MotivoIndisponibilidade { get; init; }

    // Peso e dimensões (spec 020): é o que IFreteService.Cotar precisa por
    // item, e só o Produto as carrega — nem ItemDoCarrinhoDTO (ProdutoId +
    // Quantidade, carrinho de visitante) nem um DTO novo teriam de onde vir
    // sem uma consulta extra. Correção ao plano original, que previa
    // ItemDoCarrinhoDTO como parâmetro de Cotar.
    public decimal Peso { get; init; }

    public decimal Altura { get; init; }

    public decimal Largura { get; init; }

    public decimal Comprimento { get; init; }

    public bool Disponivel => MotivoIndisponibilidade == MotivoIndisponibilidade.Nenhum;
}
