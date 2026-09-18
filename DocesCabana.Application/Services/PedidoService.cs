using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly ICarrinhoService _carrinhoService;
    private readonly IItemCarrinhoRepository _itemCarrinhoRepository;
    private readonly IEnderecoService _enderecoService;
    private readonly IFreteService _freteService;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoService(
        ICarrinhoService carrinhoService,
        IItemCarrinhoRepository itemCarrinhoRepository,
        IEnderecoService enderecoService,
        IFreteService freteService,
        IPedidoRepository pedidoRepository,
        IUnitOfWork unitOfWork)
    {
        _carrinhoService = carrinhoService;
        _itemCarrinhoRepository = itemCarrinhoRepository;
        _enderecoService = enderecoService;
        _freteService = freteService;
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PassoDoFechamentoDTO> MontarPasso(
        PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId, int? servicoDeEntregaId = null)
    {
        var autenticado = usuarioId is not null;

        var passosVisiveis = autenticado
            ? new[] { PassoDoFechamento.Carrinho, PassoDoFechamento.Endereco, PassoDoFechamento.Pagamento }
            : new[] { PassoDoFechamento.Carrinho, PassoDoFechamento.Conta, PassoDoFechamento.Endereco, PassoDoFechamento.Pagamento };

        IReadOnlyList<EnderecoDTO> enderecos = [];
        Guid? enderecoSelecionadoId = null;
        CotacaoDeFreteDTO? cotacao = null;
        int? servicoSelecionadoId = null;

        var precisaDeEndereco = autenticado && passo is PassoDoFechamento.Endereco or PassoDoFechamento.Pagamento;
        if (precisaDeEndereco)
        {
            enderecos = await _enderecoService.ListarDoUsuario(usuarioId!.Value);
            enderecoSelecionadoId = enderecoId ?? enderecos.FirstOrDefault(e => e.Padrao)?.EnderecoId;

            var enderecoEscolhido = enderecos.FirstOrDefault(e => e.EnderecoId == enderecoSelecionadoId);
            var disponiveis = carrinho.Linhas.Where(l => l.Disponivel).ToList();

            if (enderecoEscolhido is not null && disponiveis.Count > 0)
                cotacao = await _freteService.Cotar(enderecoEscolhido.CEP, disponiveis);

            if (cotacao is { Opcoes.Count: > 0 })
            {
                servicoSelecionadoId = servicoDeEntregaId is not null && cotacao.Opcoes.Any(o => o.ServicoId == servicoDeEntregaId)
                    ? servicoDeEntregaId
                    : cotacao.Opcoes.OrderBy(o => o.Preco).First().ServicoId;
            }
        }

        return new PassoDoFechamentoDTO
        {
            PassoAtivo = passo,
            PassosVisiveis = passosVisiveis,
            Carrinho = carrinho,
            Enderecos = enderecos,
            EnderecoSelecionadoId = enderecoSelecionadoId,
            ServicoDeEntregaSelecionadoId = servicoSelecionadoId,
            Cotacao = cotacao
        };
    }

    public async Task<ResultadoDoFechamentoDTO> Fechar(Guid usuarioId, FechamentoDePedidoDTO dados)
    {
        var carrinho = await _carrinhoService.ObterDoUsuario(usuarioId);

        if (carrinho.Linhas.Count == 0)
            return ResultadoDoFechamentoDTO.ParaRecusa("Seu carrinho está vazio.");

        var indisponivel = carrinho.Linhas.FirstOrDefault(l => !l.Disponivel);
        if (indisponivel is not null)
            return ResultadoDoFechamentoDTO.ParaRecusa(
                $"{indisponivel.Nome} não está mais disponível e precisa ser removido do carrinho para continuar.",
                itemIndisponivel: indisponivel.Nome);

        if (carrinho.Subtotal != dados.ValorDosProdutosExibido)
            return ResultadoDoFechamentoDTO.ParaRecusa(
                "O valor dos produtos mudou desde a última vez que você revisou. Confira o valor atual.",
                valorDosProdutosAtual: carrinho.Subtotal);

        var endereco = await _enderecoService.BuscarDoUsuario(dados.EnderecoId, usuarioId);

        var cotacao = await _freteService.Cotar(endereco.CEP, carrinho.Linhas);
        if (cotacao.Opcoes.Count == 0)
            return ResultadoDoFechamentoDTO.ParaRecusa(
                cotacao.Mensagem ?? "Não foi possível confirmar a entrega agora. Tente novamente em instantes.");

        var opcaoEscolhida = cotacao.Opcoes.FirstOrDefault(o => o.ServicoId == dados.ServicoDeEntregaId);
        if (opcaoEscolhida is null)
            return ResultadoDoFechamentoDTO.ParaRecusa(
                "A opção de entrega escolhida não está mais disponível. Escolha outra.");

        if (opcaoEscolhida.Preco != dados.ValorDoFreteExibido)
            return ResultadoDoFechamentoDTO.ParaRecusa(
                "O valor do frete mudou desde a última vez que você revisou. Confira o valor atual.",
                valorDoFreteAtual: opcaoEscolhida.Preco);

        var valorTotal = carrinho.Subtotal + opcaoEscolhida.Preco;

        var pedido = new Pedido(
            usuarioId, dados.EnderecoId, valorTotal, opcaoEscolhida.Preco,
            opcaoEscolhida.Transportadora, opcaoEscolhida.Servico,
            opcaoEscolhida.PrazoMinimoEmDias, opcaoEscolhida.PrazoMaximoEmDias);

        foreach (var linha in carrinho.Linhas)
            pedido.AcrescentarItem(linha.ProdutoId, linha.Quantidade, linha.PrecoUnitario);

        var pagamento = new Pagamento(pedido.PedidoId, dados.MetodoPagamento, valorTotal);

        await _pedidoRepository.AdicionarComPagamento(pedido, pagamento);
        await EsvaziarSemSalvar(usuarioId);

        await _unitOfWork.SalvarAlteracoes();

        return ResultadoDoFechamentoDTO.ParaSucesso(pedido.PedidoId);
    }

    public async Task<ConfirmacaoDePedidoDTO?> ObterConfirmacao(Guid pedidoId, Guid usuarioId)
    {
        var pedido = await _pedidoRepository.Buscar(pedidoId, usuarioId);
        if (pedido is null)
            return null;

        var pagamento = await _pedidoRepository.BuscarPagamentoPorPedido(pedidoId);

        return PedidoMapper.ToConfirmacaoDTO(pedido, pagamento?.Metodo ?? default);
    }

    public async Task<IReadOnlyList<ResumoDePedidoDTO>> ListarDoUsuario(Guid usuarioId)
    {
        var pedidos = await _pedidoRepository.ListarPorUsuario(usuarioId);

        return pedidos
            .OrderByDescending(p => p.Data)
            .Select(PedidoMapper.ToResumoDTO)
            .ToList();
    }

    public async Task<DetalheDePedidoDTO> BuscarDetalhe(Guid pedidoId, Guid usuarioId)
    {
        var pedido = await _pedidoRepository.Buscar(pedidoId, usuarioId)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        var pagamento = await _pedidoRepository.BuscarPagamentoPorPedido(pedidoId);

        return PedidoMapper.ToDetalheDTO(pedido, pagamento);
    }

    private async Task EsvaziarSemSalvar(Guid usuarioId)
    {
        var itens = await _itemCarrinhoRepository.BuscarPorUsuario(usuarioId);
        foreach (var item in itens)
            _itemCarrinhoRepository.Remover(item);
    }
}
