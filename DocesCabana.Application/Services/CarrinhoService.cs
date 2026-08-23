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
            // RF-03/RN-01: acrescentar o que já está soma, não duplica.
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

        // RN-02: reduzir abaixo de 1 remove o item; acima do teto satura em
        // 99 (mesmo padrão de CatalogoService.Montar saneando a página).
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

    // ── Carrinho avulso (Fase 6) — mesmas regras da versão persistida
    // (RN-01/RN-02/RN-06), aplicadas sobre uma lista em vez do banco. Quem
    // guarda a lista (a sessão) não sabe nada disso — só lê e escreve JSON
    // (Helpers/CarrinhoDaSessao, na MVC). ────────────────────────────────

    public async Task<CarrinhoDTO> MontarAvulso(IReadOnlyList<ItemDoCarrinhoDTO> itens)
    {
        var pares = new List<(Produto Produto, short Quantidade)>();
        foreach (var item in itens)
        {
            // Produto não é excluível nesta base (só inativado) — um
            // produto ausente aqui seria corrupção externa da sessão, não
            // um caso de negócio previsto; ignora em vez de derrubar a
            // tela inteira por isso.
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
            // RF-03/RN-01: acrescentar o que já está soma, não duplica.
            // RN-02: a soma nunca ultrapassa o teto — corta, não recusa.
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

        // RN-02: reduzir abaixo de 1 remove o item; acima do teto satura.
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

    // ── Fusão (Fase 7) ──────────────────────────────────────────────────

    public async Task Fundir(Guid usuarioId, IReadOnlyList<ItemDoCarrinhoDTO> itensDaSessao)
    {
        if (itensDaSessao.Count == 0)
            return;

        foreach (var itemDaSessao in itensDaSessao)
        {
            var existente = await _itemCarrinhoRepository.Buscar(usuarioId, itemDaSessao.ProdutoId);
            if (existente is not null)
            {
                // RN-05: as quantidades do mesmo produto se somam, limitadas
                // ao teto (RN-02) — nenhuma disponibilidade é checada aqui:
                // um item já guardado pode estar indisponível e continua no
                // carrinho normalmente (RN-07); o mesmo vale para o avulso.
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

        // RN-06: os dois motivos de indisponibilidade são igualmente
        // incompráveis (RF-04) — a mensagem de qual é fica por conta do
        // controlador/tela, aqui basta recusar.
        if (!produto.DisponivelParaCompra())
            throw new InvalidOperationException("Este produto não está disponível para compra.");

        return produto;
    }
}
