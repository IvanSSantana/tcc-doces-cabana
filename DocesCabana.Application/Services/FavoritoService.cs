using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Services;

public class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _favoritoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FavoritoService(
        IFavoritoRepository favoritoRepository,
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork)
    {
        _favoritoRepository = favoritoRepository;
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    // RN-01: favorito é um interruptor, não um contador — pedir de novo o
    // que já está favoritado desfaz.
    public async Task<bool> Alternar(Guid produtoId, Guid usuarioId)
    {
        var produto = await _produtoRepository.BuscarPorId(produtoId);

        // Produto inativo não existe do lado de fora (RN-01 da 012) — o
        // mesmo defeito de favoritar um produto que não existe.
        if (produto is null || produto.Status == ProdutoStatus.Inativo)
            throw new KeyNotFoundException($"Produto com ID {produtoId} não encontrado.");

        var existente = await _favoritoRepository.Buscar(produtoId, usuarioId);

        if (existente is not null)
        {
            _favoritoRepository.Remover(existente);
            await _unitOfWork.SalvarAlteracoes();
            return false;
        }

        await _favoritoRepository.Adicionar(new Domain.Entities.Favorito(produtoId, usuarioId));
        await _unitOfWork.SalvarAlteracoes();
        return true;
    }

    public async Task<List<ProdutoDTO>> ListarDoUsuario(Guid usuarioId)
    {
        var favoritos = await _favoritoRepository.BuscarPorUsuario(usuarioId);

        // RN-03: produto que saiu do catálogo público some da lista, mas o
        // favorito em si não é apagado — volta a aparecer se o produto
        // reativar.
        var produtosDisponiveis = favoritos
            .Select(f => f.Produto!)
            .Where(p => p.Status != ProdutoStatus.Inativo)
            .ToList();

        // Todos aqui são favoritos por definição — o conjunto passado ao
        // mapeador é o próprio conjunto de identificadores da lista.
        var idsFavoritados = produtosDisponiveis.Select(p => p.ProdutoId).ToHashSet();

        return ProdutoMapper.ToDTO(produtosDisponiveis, idsFavoritados);
    }
}
