namespace DocesCabana.Domain.Contracts;

public interface ITransaction : IAsyncDisposable
{
    Task Confirmar(CancellationToken cancellationToken = default);

    Task Reverter(CancellationToken cancellationToken = default);
}
