using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IProdutoService
{
    Task<List<ProdutoDTO>> BuscarTodosProdutos();

    Task<ProdutoDTO> BuscarProdutoPorId(Guid id);

    Task<ProdutoDTO> Cadastrar(ProdutoDTO dto);

    // RF-04/RF-05/RF-09 (spec 019): "os oito mais bem avaliados", pedidos ao
    // armazenamento com esse limite — não a loja inteira filtrada em memória.
    // usuarioId nulo (visitante) não marca favorito nenhum (RF-10).
    Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null);

    /// <summary>Lança <see cref="KeyNotFoundException"/> para produto inexistente ou inativo — RF-03, RF-04.</summary>
    Task<ProdutoDetalheDTO> BuscarDetalhe(Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual);
}
