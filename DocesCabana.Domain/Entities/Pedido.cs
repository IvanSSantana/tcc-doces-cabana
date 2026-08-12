using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

public class Pedido
{
    public Guid PedidoId { get; private set; }

    // Sem navegação: Usuario vive na Infrastructure (RQ-02 da spec 003).
    public Guid UsuarioId { get; private set; }

    public Guid EnderecoEntregaId { get; private set; }

    public bool PagamentoAprovado { get; private set; }

    public decimal Valor { get; private set; }

    public PedidoStatus Status { get; private set; }

    public DateTime Data { get; private set; }

    // Navegação filho -> pai. Sem coleção de itens nesta entrega — quem
    // gerencia o agregado (calcula total, adiciona/remove item) é decisão da
    // spec de carrinho, não desta (RQ-11 da spec 003).
    public Endereco? EnderecoEntrega { get; private set; }

    protected Pedido() { }

    public Pedido(Guid usuarioId, Guid enderecoEntregaId, decimal valor, Guid id = default)
    {
        ValidarUsuario(usuarioId);
        ValidarEndereco(enderecoEntregaId);
        ValidarValor(valor);

        PedidoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        UsuarioId = usuarioId;
        EnderecoEntregaId = enderecoEntregaId;
        Valor = valor;
        Status = PedidoStatus.Pendente;
        PagamentoAprovado = false;
        Data = DateTime.UtcNow;
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
}
