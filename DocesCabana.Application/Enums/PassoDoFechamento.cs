namespace DocesCabana.Application.Enums;

// A ordem declarada aqui é a ordem visual dos passos (spec 022, RF-01).
// Carrinho é o repouso — onde a pessoa já está antes de avançar (CA-01).
public enum PassoDoFechamento
{
    Carrinho,
    Conta,
    Endereco,
    Pagamento
}
