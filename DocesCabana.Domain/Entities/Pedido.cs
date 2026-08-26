using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

public class Pedido
{
    public Guid PedidoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Guid EnderecoEntregaId { get; private set; }

    public bool PagamentoAprovado { get; private set; }

    public decimal Valor { get; private set; }

    // Dados da entrega, congelados no fechamento (spec 022, RN-01) — mudar o
    // preço do frete ou a transportadora depois não muda o que a pessoa já
    // combinou. ValorDoFrete existe separado de Valor, em vez de derivado por
    // subtração, porque quando cupom ou promoção entrarem no total, a
    // subtração deixaria de bater e o erro apareceria silenciosamente numa
    // tela, não num teste (plano §6).
    public decimal ValorDoFrete { get; private set; }

    public string Transportadora { get; private set; } = default!;

    public string Servico { get; private set; } = default!;

    public int PrazoMinimoEmDias { get; private set; }

    public int PrazoMaximoEmDias { get; private set; }

    public PedidoStatus Status { get; private set; }

    public DateTime Data { get; private set; }

    // Pedido é a raiz do agregado (spec 022, plano §3) — decisão adiada desde
    // a modelagem, registrada ali como "quem gerencia o agregado é decisão da
    // spec de carrinho". É esta. Campo de apoio porque a coleção exposta
    // precisa ser somente-leitura (Princípio II — estado só muda por método
    // de intenção).
    private readonly List<ItemPedido> _itens = [];

    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    // Navegações filho -> pai. Usuario é do domínio (spec 004).
    public Endereco? EnderecoEntrega { get; private set; }

    public Usuario? Usuario { get; private set; }

    protected Pedido() { }

    public Pedido(
        Guid usuarioId, Guid enderecoEntregaId, decimal valor, decimal valorDoFrete,
        string transportadora, string servico, int prazoMinimoEmDias, int prazoMaximoEmDias, Guid id = default)
    {
        ValidarUsuario(usuarioId);
        ValidarEndereco(enderecoEntregaId);
        ValidarValor(valor);
        ValidarValorDoFrete(valorDoFrete);
        ValidarTransportadora(transportadora);
        ValidarServico(servico);
        ValidarPrazos(prazoMinimoEmDias, prazoMaximoEmDias);

        PedidoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        UsuarioId = usuarioId;
        EnderecoEntregaId = enderecoEntregaId;
        Valor = valor;
        ValorDoFrete = valorDoFrete;
        Transportadora = transportadora;
        Servico = servico;
        PrazoMinimoEmDias = prazoMinimoEmDias;
        PrazoMaximoEmDias = prazoMaximoEmDias;
        Status = PedidoStatus.Pendente;
        PagamentoAprovado = false;
        Data = DateTime.UtcNow;
    }

    public void AcrescentarItem(Guid produtoId, short quantidade, decimal precoUnitario) =>
        _itens.Add(new ItemPedido(PedidoId, produtoId, quantidade, precoUnitario));

    // Método, não propriedade: propriedade computada o EF Core tentaria
    // mapear para coluna — mesma razão de Produto.DisponivelParaCompra()
    // (spec 022, plano §4). Curto o bastante para ser ditado (RF-23).
    public string NumeroVisivel() => PedidoId.ToString("N")[..8].ToUpperInvariant();

    // A situação de um pedido criado pela aplicação só avança quando a
    // processadora de pagamento existir — nenhum caminho real desta entrega
    // chama estes métodos (spec §10). Existem para os pedidos SEMEADOS
    // (DbInitializer), que representam compras passadas e por isso nascem
    // com situação variada de propósito — inclusive cancelado, que RN-05
    // exige existir para provar que venda cancelada não conta.
    public void Cancelar() => Status = PedidoStatus.Cancelado;

    public void Confirmar() => Status = PedidoStatus.Confirmado;

    public void MarcarComoEnviado() => Status = PedidoStatus.Enviado;

    public void MarcarComoEntregue() => Status = PedidoStatus.Entregue;

    private void ValidarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }

    private void ValidarEndereco(Guid enderecoEntregaId)
    {
        if (enderecoEntregaId == Guid.Empty)
            throw new ArgumentException("Endereço de entrega inválido.", nameof(enderecoEntregaId));
    }

    private void ValidarValor(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo.", nameof(valor));
    }

    private void ValidarValorDoFrete(decimal valorDoFrete)
    {
        if (valorDoFrete < 0)
            throw new ArgumentException("Valor do frete não pode ser negativo.", nameof(valorDoFrete));
    }

    private void ValidarTransportadora(string transportadora)
    {
        if (string.IsNullOrWhiteSpace(transportadora))
            throw new ArgumentException("Transportadora é obrigatória.", nameof(transportadora));
    }

    private void ValidarServico(string servico)
    {
        if (string.IsNullOrWhiteSpace(servico))
            throw new ArgumentException("Serviço de entrega é obrigatório.", nameof(servico));
    }

    private void ValidarPrazos(int prazoMinimoEmDias, int prazoMaximoEmDias)
    {
        if (prazoMinimoEmDias <= 0)
            throw new ArgumentException("Prazo mínimo deve ser maior que zero.", nameof(prazoMinimoEmDias));

        if (prazoMaximoEmDias < prazoMinimoEmDias)
            throw new ArgumentException("Prazo máximo não pode ser menor que o prazo mínimo.", nameof(prazoMaximoEmDias));
    }
}
