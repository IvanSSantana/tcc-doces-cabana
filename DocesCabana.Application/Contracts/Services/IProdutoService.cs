using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IProdutoService
{
    Task<List<ProdutoDTO>> BuscarTodosProdutos();

    Task<ProdutoDTO> BuscarProdutoPorId(Guid id);

    Task<ProdutoDTO> Cadastrar(ProdutoDTO dto);

    /// <summary>Lança <see cref="KeyNotFoundException"/> para produto inexistente ou inativo — RF-03, RF-04.</summary>
    Task<ProdutoDetalheDTO> BuscarDetalhe(Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual);
}
