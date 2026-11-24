namespace DocesCabana.Application.Contracts.Services;

/// <summary>
/// Cache de leitura compartilhado entre requisições. Só guarda dado que muda por ação
/// do administrador; quem grava esse dado remove a chave logo depois de salvar.
/// O valor guardado é a mesma instância para todos os leitores: não deve ser alterado.
/// </summary>
public interface ICacheDaAplicacao
{
    Task<T> ObterOuCriar<T>(string chave, Func<Task<T>> fabrica, TimeSpan duracao);

    void Remover(string chave);
}
