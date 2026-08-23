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

    public bool Disponivel => MotivoIndisponibilidade == MotivoIndisponibilidade.Nenhum;
}
