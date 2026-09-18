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

    public async Task<bool> Alternar(Guid produtoId, Guid usuarioId)
    {
        var produto = await _produtoRepository.BuscarPorId(produtoId);

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

        var produtosDisponiveis = favoritos
            .Select(f => f.Produto!)
            .Where(p => p.Status != ProdutoStatus.Inativo)
            .ToList();

        var idsFavoritados = produtosDisponiveis.Select(p => p.ProdutoId).ToHashSet();

        return ProdutoMapper.ToDTO(produtosDisponiveis, idsFavoritados);
    }
}
