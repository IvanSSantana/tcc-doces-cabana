using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Services;

public class CarrinhoService : ICarrinhoService
{
    private readonly IItemCarrinhoRepository _itemCarrinhoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CarrinhoService(
        IItemCarrinhoRepository itemCarrinhoRepository,
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork)
    {
        _itemCarrinhoRepository = itemCarrinhoRepository;
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CarrinhoDTO> ObterDoUsuario(Guid usuarioId)
    {
        var itens = await _itemCarrinhoRepository.BuscarPorUsuario(usuarioId);
        return CarrinhoMapper.ToDTO(itens);
    }

    public async Task Acrescentar(Guid usuarioId, Guid produtoId, short quantidade)
    {
        var produto = await BuscarProdutoDisponivel(produtoId);

        var existente = await _itemCarrinhoRepository.Buscar(usuarioId, produtoId);
        if (existente is not null)
        {
            existente.Acrescentar(quantidade);
        }
        else
        {
            await _itemCarrinhoRepository.Adicionar(new ItemCarrinho(usuarioId, produtoId, quantidade));
        }

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task AlterarQuantidade(Guid usuarioId, Guid produtoId, short quantidade)
    {
        var item = await _itemCarrinhoRepository.Buscar(usuarioId, produtoId);
        if (item is null)
            throw new KeyNotFoundException("Item não encontrado no carrinho.");

        if (quantidade < ItemCarrinho.QuantidadeMinima)
        {
            _itemCarrinhoRepository.Remover(item);
        }
        else
        {
            var quantidadeSaneada = (short)Math.Min(quantidade, ItemCarrinho.QuantidadeMaxima);
            item.AlterarQuantidade(quantidadeSaneada);
        }

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Remover(Guid usuarioId, Guid produtoId)
    {
        var item = await _itemCarrinhoRepository.Buscar(usuarioId, produtoId);
        if (item is null)
            throw new KeyNotFoundException("Item não encontrado no carrinho.");

        _itemCarrinhoRepository.Remover(item);
        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task<int> ContarItens(Guid usuarioId) =>
        await _itemCarrinhoRepository.ContarItens(usuarioId);

    public async Task Esvaziar(Guid usuarioId)
    {
        var itens = await _itemCarrinhoRepository.BuscarPorUsuario(usuarioId);
        if (itens.Count == 0)
            return;

        foreach (var item in itens)
            _itemCarrinhoRepository.Remover(item);

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task<CarrinhoDTO> MontarAvulso(IReadOnlyList<ItemDoCarrinhoDTO> itens)
    {
        var pares = new List<(Produto Produto, short Quantidade)>();
        foreach (var item in itens)
        {
            var produto = await _produtoRepository.BuscarPorId(item.ProdutoId);
            if (produto is not null)
                pares.Add((produto, item.Quantidade));
        }

        return CarrinhoMapper.Montar(pares);
    }

    public async Task<IReadOnlyList<ItemDoCarrinhoDTO>> AcrescentarAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade)
    {
        await BuscarProdutoDisponivel(produtoId);

        var lista = itens.ToList();
        var indice = lista.FindIndex(i => i.ProdutoId == produtoId);

        if (indice >= 0)
        {
            var somada = (short)Math.Min(lista[indice].Quantidade + quantidade, ItemCarrinho.QuantidadeMaxima);
            lista[indice] = lista[indice] with { Quantidade = somada };
        }
        else
        {
            var quantidadeSaneada = (short)Math.Min(quantidade, ItemCarrinho.QuantidadeMaxima);
            lista.Add(new ItemDoCarrinhoDTO(produtoId, quantidadeSaneada));
        }

        return lista;
    }

    public IReadOnlyList<ItemDoCarrinhoDTO> AlterarQuantidadeAvulsa(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade)
    {
        var lista = itens.ToList();
        var indice = lista.FindIndex(i => i.ProdutoId == produtoId);
        if (indice < 0)
            throw new KeyNotFoundException("Item não encontrado no carrinho.");

        if (quantidade < ItemCarrinho.QuantidadeMinima)
        {
            lista.RemoveAt(indice);
        }
        else
        {
            var quantidadeSaneada = (short)Math.Min(quantidade, ItemCarrinho.QuantidadeMaxima);
            lista[indice] = lista[indice] with { Quantidade = quantidadeSaneada };
        }

        return lista;
    }

    public IReadOnlyList<ItemDoCarrinhoDTO> RemoverAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId)
    {
        var lista = itens.ToList();
        var indice = lista.FindIndex(i => i.ProdutoId == produtoId);
        if (indice < 0)
            throw new KeyNotFoundException("Item não encontrado no carrinho.");

        lista.RemoveAt(indice);
        return lista;
    }

    public async Task Fundir(Guid usuarioId, IReadOnlyList<ItemDoCarrinhoDTO> itensDaSessao)
    {
        if (itensDaSessao.Count == 0)
            return;

        foreach (var itemDaSessao in itensDaSessao)
        {
            var existente = await _itemCarrinhoRepository.Buscar(usuarioId, itemDaSessao.ProdutoId);
            if (existente is not null)
            {
                existente.Acrescentar(itemDaSessao.Quantidade);
            }
            else
            {
                await _itemCarrinhoRepository.Adicionar(
                    new ItemCarrinho(usuarioId, itemDaSessao.ProdutoId, itemDaSessao.Quantidade));
            }
        }

        await _unitOfWork.SalvarAlteracoes();
    }

    private async Task<Produto> BuscarProdutoDisponivel(Guid produtoId)
    {
        var produto = await _produtoRepository.BuscarPorId(produtoId);
        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {produtoId} não encontrado.");

        if (!produto.DisponivelParaCompra())
            throw new InvalidOperationException("Este produto não está disponível para compra.");

        return produto;
    }
}
