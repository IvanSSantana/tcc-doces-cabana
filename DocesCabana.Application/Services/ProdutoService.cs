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
    private readonly IFavoritoRepository _favoritoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(
        IProdutoRepository produtoRepository, IAvaliacaoService avaliacaoService,
        IFavoritoRepository favoritoRepository, IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _avaliacaoService = avaliacaoService;
        _favoritoRepository = favoritoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProdutoDTO>> BuscarTodosProdutos()
    {
        var produtos = await _produtoRepository.BuscarTodos();

        var disponiveis = produtos.Where(p => p.Status != ProdutoStatus.Inativo);

        return ProdutoMapper.ToDTO(disponiveis);
    }

    public async Task<ProdutoDTO> BuscarProdutoPorId(Guid id)
    {
        var produto = await _produtoRepository.BuscarPorId(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return ProdutoMapper.ToDTO(produto);
    }

    public async Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null)
    {
        var filtroVazio = new FiltroCatalogoDTO(
            CategoriaId: null,
            SubcategoriaIds: [],
            ApenasSemAcucar: false,
            Ordenacao: OrdenacaoCatalogo.MaisVendidos);

        var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtroVazio, pagina: 1, tamanhoDaPagina: limite);

        var idsFavoritados = usuarioId.HasValue
            ? await _favoritoRepository.IdsPorUsuario(usuarioId.Value, produtos.Select(p => p.ProdutoId))
            : [];

        return ProdutoMapper.ToDTO(produtos, idsFavoritados);
    }

    public async Task<ProdutoDTO> Cadastrar(ProdutoDTO dto)
    {
        var produto = ProdutoMapper.ToEntity(dto);
        await _produtoRepository.Adicionar(produto);
        await _unitOfWork.SalvarAlteracoes();

        return ProdutoMapper.ToDTO(produto);
    }

    public async Task Remover(Guid produtoId)
    {
        var produto = await _produtoRepository.BuscarPorId(produtoId) 
            ?? throw new KeyNotFoundException($"Produo com ID {produtoId} não encontrado.");

        _produtoRepository.Remover(produto);
        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task<ProdutoDTO> Atualizar(ProdutoDTO dto)
    {
        var produto = await _produtoRepository.BuscarPorId(dto.ProdutoId) 
            ?? throw new KeyNotFoundException($"Produto com ID {dto.ProdutoId} não encontrado.");

        var produtoAtualizado = await _produtoRepository.Atualizar(ProdutoMapper.ToEntity(dto));
        await _unitOfWork.SalvarAlteracoes();

        return ProdutoMapper.ToDTO(produtoAtualizado);
    }

    public async Task<ProdutoDetalheDTO> BuscarDetalhe(
        Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual)
    {
        var produto = await _produtoRepository.BuscarDetalhePorId(id);

        if (produto is null || produto.Status == ProdutoStatus.Inativo)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        var resumoAvaliacoes = await _avaliacaoService.ResumirPorProduto(id);
        var paginaAvaliacoes = await _avaliacaoService.ListarPorProduto(id, ordenacao, avaliacoesExibidas, usuarioAtual);

        return ProdutoDetalheMapper.ToDTO(produto, resumoAvaliacoes, paginaAvaliacoes);
    }
}
