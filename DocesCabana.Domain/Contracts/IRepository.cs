namespace DocesCabana.Domain.Contracts;

public interface IRepository<T> where T : class
{
    Task<T?> BuscarPorIdAsync(Guid id);

    Task<IEnumerable<T>> BuscarTodosAsync();

    Task AdicionarAsync(T entity);

    void Atualizar(T entity);

    void Remover(T entity);
}