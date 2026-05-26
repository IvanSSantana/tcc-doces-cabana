namespace DocesCabana.Domain.Contracts;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SalvarAlteracoes(CancellationToken cancellationToken = default);

    Task<ITransaction> IniciarTransacao(CancellationToken cancellationToken = default);

    Task ExecutarEmTransacao(Func<Task> operacao, CancellationToken cancellationToken = default);
}
