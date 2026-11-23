using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly ICarrinhoService _carrinhoService;
    private readonly IItemCarrinhoRepository _itemCarrinhoRepository;
    private readonly IEnderecoService _enderecoService;
    private readonly IFreteService _freteService;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ICupomService _cupomService;
    private readonly IAgendaDeRetiradaService _agendaDeRetiradaService;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoService(
        ICarrinhoService carrinhoService,
        IItemCarrinhoRepository itemCarrinhoRepository,
        IEnderecoService enderecoService,
        IFreteService freteService,
        IPedidoRepository pedidoRepository,
        ICupomService cupomService,
        IAgendaDeRetiradaService agendaDeRetiradaService,
        IUnitOfWork unitOfWork)
    {
        _carrinhoService = carrinhoService;
        _itemCarrinhoRepository = itemCarrinhoRepository;
        _enderecoService = enderecoService;
        _freteService = freteService;
        _pedidoRepository = pedidoRepository;
        _cupomService = cupomService;
        _agendaDeRetiradaService = agendaDeRetiradaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PassoDoFechamentoDTO> MontarPasso(
        PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId,
        int? servicoDeEntregaId = null, string? codigoDoCupom = null,
        ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega, TimeOnly? inicioDaFaixa = null)
    {
        var autenticado = usuarioId is not null;

        var passosVisiveis = autenticado
            ? new[] { PassoDoFechamento.Carrinho, PassoDoFechamento.Endereco, PassoDoFechamento.Pagamento }
            : new[] { PassoDoFechamento.Carrinho, PassoDoFechamento.Conta, PassoDoFechamento.Endereco, PassoDoFechamento.Pagamento };

        IReadOnlyList<EnderecoDTO> enderecos = [];
        Guid? enderecoSelecionadoId = null;
        CotacaoDeFreteDTO? cotacao = null;
        int? servicoSelecionadoId = null;
        RetiradaDisponivelDTO? retirada = null;
        TimeOnly? faixaSelecionada = null;

        var precisaDeEndereco = autenticado && passo is PassoDoFechamento.Endereco or PassoDoFechamento.Pagamento;

        if (precisaDeEndereco)
            retirada = await _agendaDeRetiradaService.ConsultarHoje();

        // Sem faixa hoje, a retirada fica desabilitada (D5) e o passo volta para a entrega.
        if (retirada is not { Disponivel: true })
            modalidade = ModalidadeDeEntrega.Entrega;

        var retiradaNaLoja = modalidade == ModalidadeDeEntrega.RetiradaNaLoja;

        if (precisaDeEndereco && retiradaNaLoja)
        {
            faixaSelecionada = inicioDaFaixa is not null && retirada!.Faixas.Any(f => f.Inicio == inicioDaFaixa)
                ? inicioDaFaixa
                : retirada!.Faixas.FirstOrDefault()?.Inicio;
        }
        else if (precisaDeEndereco)
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

        var freteDeReferencia = passo == PassoDoFechamento.Carrinho
            ? (carrinho.TemEntregaCalculada ? carrinho.Cotacao!.Opcoes.Min(o => o.Preco) : null)
            : retiradaNaLoja
                ? 0m
                : cotacao?.Opcoes.FirstOrDefault(o => o.ServicoId == servicoSelecionadoId)?.Preco;

        ResultadoDoCupomDTO? cupom = null;
        if (autenticado && !string.IsNullOrWhiteSpace(codigoDoCupom))
            cupom = await _cupomService.Avaliar(
                codigoDoCupom, usuarioId!.Value, carrinho, freteDeReferencia,
                precisaDeEndereco ? modalidade : ModalidadeDeEntrega.Entrega);

        return new PassoDoFechamentoDTO
        {
            Modalidade = precisaDeEndereco ? modalidade : ModalidadeDeEntrega.Entrega,
            Retirada = retirada,
            InicioDaFaixaSelecionada = faixaSelecionada,
            PassoAtivo = passo,
            PassosVisiveis = passosVisiveis,
            Carrinho = carrinho,
            Enderecos = enderecos,
            EnderecoSelecionadoId = enderecoSelecionadoId,
            ServicoDeEntregaSelecionadoId = servicoSelecionadoId,
            Cotacao = cotacao,
            Cupom = cupom,
            ValorDoFreteDeReferencia = freteDeReferencia
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

        decimal valorDoFrete;
        Func<decimal, Pedido> criarPedido;

        if (dados.Modalidade == ModalidadeDeEntrega.RetiradaNaLoja)
        {
            var retirada = await _agendaDeRetiradaService.ConsultarHoje();

            var faixa = retirada.Faixas.FirstOrDefault(f => f.Inicio == dados.InicioDaFaixa);
            if (faixa is null)
                return ResultadoDoFechamentoDTO.ParaRecusa(
                    "A faixa de retirada escolhida não está mais disponível. Escolha outra.");

            valorDoFrete = 0m;
            criarPedido = valorTotal => Pedido.ParaRetirada(
                usuarioId, valorTotal, new FaixaDeRetirada(retirada.Data, faixa.Inicio, faixa.Fim));
        }
        else
        {
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

            valorDoFrete = opcaoEscolhida.Preco;
            criarPedido = valorTotal => new Pedido(
                usuarioId, dados.EnderecoId, valorTotal, opcaoEscolhida.Preco,
                opcaoEscolhida.Transportadora, opcaoEscolhida.Servico,
                opcaoEscolhida.PrazoMinimoEmDias, opcaoEscolhida.PrazoMaximoEmDias);
        }

        ResultadoDoCupomDTO? cupom = null;
        if (!string.IsNullOrWhiteSpace(dados.CodigoDoCupom))
        {
            cupom = await _cupomService.Avaliar(dados.CodigoDoCupom, usuarioId, carrinho, valorDoFrete, dados.Modalidade);

            if (!cupom.Valido)
                return ResultadoDoFechamentoDTO.ParaRecusa(cupom.Motivo!, cupomRecusado: true);

            if (cupom.DescontoTotal != dados.DescontoDoCupomExibido)
                return ResultadoDoFechamentoDTO.ParaRecusa(
                    "O desconto do cupom mudou desde a última vez que você revisou. Confira o valor atual.",
                    descontoDoCupomAtual: cupom.DescontoTotal);
        }

        var valorTotal = carrinho.Subtotal + valorDoFrete - (cupom?.DescontoTotal ?? 0m);

        if (valorTotal <= 0)
            return ResultadoDoFechamentoDTO.ParaRecusa("Não foi possível aplicar este cupom a um pedido sem valor.");

        var pedido = criarPedido(valorTotal);

        foreach (var linha in carrinho.Linhas)
            pedido.AcrescentarItem(
                linha.ProdutoId, linha.Quantidade, linha.PrecoUnitario,
                linha.DescontoDaPromocao, linha.RotuloDaPromocao);

        if (cupom is not null)
            pedido.AplicarCupom(cupom.CupomId!.Value, cupom.Codigo, cupom.DescontoNosProdutos, cupom.DescontoNoFrete);

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

    public async Task MarcarComoEntregue(Guid pedidoId, Guid usuarioId)
    {
        var pedido = await _pedidoRepository.BuscarParaAlterar(pedidoId, usuarioId)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        pedido.MarcarComoEntregue();

        await _unitOfWork.SalvarAlteracoes();
    }

    private async Task EsvaziarSemSalvar(Guid usuarioId)
    {
        var itens = await _itemCarrinhoRepository.BuscarPorUsuario(usuarioId);
        foreach (var item in itens)
            _itemCarrinhoRepository.Remover(item);
    }
}
