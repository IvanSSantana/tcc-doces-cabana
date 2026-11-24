using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IBannerRepository : IRepository<Banner>
{
    /// <summary>
    /// Ativos, dentro do período e cujo produto de destino não está inativo, na ordem do carrossel
    /// (<see cref="Banner.Ordem"/>, depois <see cref="Banner.DataInicio"/>). Vem sem rastreamento.
    /// </summary>
    Task<List<Banner>> ListarVigentes(DateOnly hoje);

    Task<List<Banner>> ListarParaGestao();
}
