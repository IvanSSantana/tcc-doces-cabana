namespace DocesCabana.Domain.Contracts;

public interface IUnitOfWork
{
    Task<int> SalvarAlteracoes(CancellationToken cancellationToken = default);
}
