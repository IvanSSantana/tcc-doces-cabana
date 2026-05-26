namespace DocesCabana.Domain.Contracts;

public interface IRepository<T> where T : class
{
    Task<T?> BuscarPorId(Guid id);

    Task<IEnumerable<T>> BuscarTodos();

    Task Adicionar(T entity);

    void Atualizar(T entity);

    void Remover(T entity);
}