using DocesCabana.Domain.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocesCabana.Infrastructure.Repositories;

internal sealed class TransactionEF : ITransaction
{
    private readonly IDbContextTransaction _transacao;

    public TransactionEF(IDbContextTransaction transacao)
    {
        _transacao = transacao;
    }

    public Task Confirmar(CancellationToken cancellationToken = default) =>
        _transacao.CommitAsync(cancellationToken);

    public Task Reverter(CancellationToken cancellationToken = default) =>
        _transacao.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() =>
        _transacao.DisposeAsync();
}
