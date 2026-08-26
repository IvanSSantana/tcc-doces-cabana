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

        // RN-01/RF-25 (spec 012): produto inativo não existe do lado de fora
        // em nenhuma listagem — defeito encontrado durante a especificação,
        // a vitrine da home levava a um 404 ao clicar num produto inativo.
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

    // RF-04/RF-05/RF-09 (spec 019, plano §5): reaproveita a mesma consulta
    // paginada do catálogo, com filtro vazio (nenhuma categoria, nenhuma
    // subcategoria, sem termo) e ordenação por avaliação — o Skip/Take de
    // BuscarPaginaDoCatalogo já traduz para LIMIT/OFFSET no banco, então só
    // os `limite` produtos pedidos chegam à memória. A mesma regra de RN-02
    // que exclui produto inativo do catálogo se aplica aqui, porque é a
    // mesma consulta.
    public async Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null)
    {
        // RF-24 (spec 022): a vitrine passa a pedir os mais vendidos, não
        // mais os melhor avaliados (RF-04/RF-09 da 019, superado aqui).
        var filtroVazio = new FiltroCatalogoDTO(
            CategoriaId: null,
            SubcategoriaIds: [],
            ApenasSemAcucar: false,
            Ordenacao: OrdenacaoCatalogo.MaisVendidos);

        var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtroVazio, pagina: 1, tamanhoDaPagina: limite);

        // RF-10: visitante não tem favorito nenhum — nem consultamos.
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
