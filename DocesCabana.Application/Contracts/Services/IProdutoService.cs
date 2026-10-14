using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IProdutoService
{
    Task<List<ProdutoDTO>> BuscarTodosProdutos();

    Task<ProdutoDTO> BuscarProdutoPorId(Guid id);

    Task<ProdutoDTO> Cadastrar(ProdutoDTO dto);

    Task Remover(Guid produtoId);

    Task<ProdutoDTO> Atualizar(ProdutoDTO dto);

    Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null);

    Task<ProdutoDetalheDTO> BuscarDetalhe(Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual);
}
