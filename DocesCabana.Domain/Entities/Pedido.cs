using DocesCabana.Domain.Enums;
using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Domain.Entities;

public class Pedido
{
    public Guid PedidoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public ModalidadeDeEntrega Modalidade { get; private set; }

    public Guid? EnderecoEntregaId { get; private set; }

    public bool PagamentoAprovado { get; private set; }

    public decimal Valor { get; private set; }

    public decimal ValorDoFrete { get; private set; }

    public string? Transportadora { get; private set; }

    public string? Servico { get; private set; }

    public int? PrazoMinimoEmDias { get; private set; }

    public int? PrazoMaximoEmDias { get; private set; }

    public DateOnly? DataDaRetirada { get; private set; }

    public TimeOnly? InicioDaRetirada { get; private set; }

    public TimeOnly? FimDaRetirada { get; private set; }

    public PedidoStatus Status { get; private set; }

    public DateTime Data { get; private set; }

    public Guid? CupomId { get; private set; }

    public Cupom? Cupom { get; private set; }

    public string? CodigoDoCupom { get; private set; }

    public decimal DescontoDoCupom { get; private set; }

    public decimal DescontoNoFrete { get; private set; }

    private readonly List<ItemPedido> _itens = [];

    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

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

        Modalidade = ModalidadeDeEntrega.Entrega;
        UsuarioId = usuarioId;
        EnderecoEntregaId = enderecoEntregaId;
        Valor = valor;
        ValorDoFrete = valorDoFrete;
        Transportadora = transportadora;
        Servico = servico;
        PrazoMinimoEmDias = prazoMinimoEmDias;
        PrazoMaximoEmDias = prazoMaximoEmDias;
        Status = PedidoStatus.Confirmado;
        PagamentoAprovado = false;
        Data = DateTime.UtcNow;
    }

    /// <summary>Pedido retirado na loja: sem endereço nem transportadora, frete zero.</summary>
    public static Pedido ParaRetirada(Guid usuarioId, decimal valor, FaixaDeRetirada faixa, Guid id = default)
    {
        ArgumentNullException.ThrowIfNull(faixa, nameof(faixa));

        var pedido = new Pedido();
        pedido.ValidarUsuario(usuarioId);
        pedido.ValidarValor(valor);

        if (faixa.Fim <= faixa.Inicio)
            throw new ArgumentException("O fim da faixa de retirada precisa ser depois do início.", nameof(faixa));

        pedido.PedidoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        pedido.Modalidade = ModalidadeDeEntrega.RetiradaNaLoja;
        pedido.UsuarioId = usuarioId;
        pedido.Valor = valor;
        pedido.ValorDoFrete = 0m;
        pedido.DataDaRetirada = faixa.Data;
        pedido.InicioDaRetirada = faixa.Inicio;
        pedido.FimDaRetirada = faixa.Fim;
        pedido.Status = PedidoStatus.Confirmado;
        pedido.PagamentoAprovado = false;
        pedido.Data = DateTime.UtcNow;

        return pedido;
    }

    public void AcrescentarItem(
        Guid produtoId,
        short quantidade,
        decimal precoUnitario,
        decimal descontoDaPromocao = 0m,
        string? rotuloDaPromocao = null) =>
        _itens.Add(new ItemPedido(PedidoId, produtoId, quantidade, precoUnitario, descontoDaPromocao, rotuloDaPromocao));

    public void AplicarCupom(Guid cupomId, string codigo, decimal descontoNosProdutos, decimal descontoNoFrete)
    {
        if (cupomId == Guid.Empty)
            throw new ArgumentException("Cupom inválido.", nameof(cupomId));

        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código do cupom é obrigatório.", nameof(codigo));

        if (descontoNosProdutos < 0 || descontoNoFrete < 0)
            throw new ArgumentException("Desconto do cupom não pode ser negativo.");

        if (descontoNosProdutos > SomarLinhas())
            throw new ArgumentException("Desconto do cupom não pode passar do valor dos produtos.", nameof(descontoNosProdutos));

        if (descontoNoFrete > ValorDoFrete)
            throw new ArgumentException("Desconto no frete não pode passar do valor do frete.", nameof(descontoNoFrete));

        CupomId = cupomId;
        CodigoDoCupom = codigo;
        DescontoDoCupom = descontoNosProdutos;
        DescontoNoFrete = descontoNoFrete;
    }

    public decimal ValorDosProdutos() => SomarLinhas() - DescontoDoCupom;

    private decimal SomarLinhas() => _itens.Sum(i => i.CalcularValorDaLinha());

    public string NumeroVisivel() => PedidoId.ToString("N")[..8].ToUpperInvariant();

    public void Cancelar() => Status = PedidoStatus.Cancelado;

    public void Confirmar() => Status = PedidoStatus.Confirmado;

    public void MarcarComoEnviado() => Status = PedidoStatus.Enviado;

    public void MarcarComoEntregue()
    {
        if (Status == PedidoStatus.Cancelado)
            throw new InvalidOperationException("Pedido cancelado não pode ser marcado como entregue.");

        if (Status == PedidoStatus.Entregue)
            throw new InvalidOperationException("Este pedido já foi marcado como entregue.");

        Status = PedidoStatus.Entregue;
    }

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
