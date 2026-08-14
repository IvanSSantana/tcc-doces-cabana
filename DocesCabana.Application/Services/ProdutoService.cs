using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IAvaliacaoService _avaliacaoService;
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IProdutoRepository produtoRepository, IAvaliacaoService avaliacaoService, IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _avaliacaoService = avaliacaoService;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProdutoDTO>> BuscarTodosProdutos()
    {
        var produtos = await _produtoRepository.BuscarTodos();

        return ProdutoMapper.ToDTO(produtos);
    }

    public async Task<ProdutoDTO> BuscarProdutoPorId(Guid id)
    {
        var produto = await _produtoRepository.BuscarPorId(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return ProdutoMapper.ToDTO(produto);
    }

    public async Task<ProdutoDTO> Cadastrar(ProdutoDTO dto)
    {
        var produto = ProdutoMapper.ToEntity(dto);
        await _produtoRepository.Adicionar(produto);
        await _unitOfWork.SalvarAlteracoes();

        return ProdutoMapper.ToDTO(produto);
    }

    public async Task<ProdutoDetalheDTO> BuscarDetalhe(
        Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual)
    {
        var produto = await _produtoRepository.BuscarDetalhePorId(id);

        // RF-04/RN-12: produto inativo responde "não encontrado", como se
        // não existisse — mesma exceção do id que não corresponde a nada.
        if (produto is null || produto.Status == ProdutoStatus.Inativo)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        var resumoAvaliacoes = await _avaliacaoService.ResumirPorProduto(id);
        var paginaAvaliacoes = await _avaliacaoService.ListarPorProduto(id, ordenacao, avaliacoesExibidas, usuarioAtual);

        return ProdutoDetalheMapper.ToDTO(produto, resumoAvaliacoes, paginaAvaliacoes);
    }
}
