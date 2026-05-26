using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.DatabaseContext;

namespace DocesCabana.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DocesCabanaDbContext _context;

    public UnitOfWork(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public Task<int> SalvarAlteracoes(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<ITransaction> IniciarTransacao(CancellationToken cancellationToken = default)
    {
        var transacao = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new TransactionEF(transacao);
    }

    public async Task ExecutarEmTransacao(Func<Task> operacao, CancellationToken cancellationToken = default)
    {
        await using var transacao = await IniciarTransacao(cancellationToken);

        try
        {
            await operacao();
            await SalvarAlteracoes(cancellationToken);
            await transacao.Confirmar(cancellationToken);
        }
        catch
        {
            await transacao.Reverter(cancellationToken);
            throw;
        }
    }

    public ValueTask DisposeAsync() =>
        _context.DisposeAsync();
}
