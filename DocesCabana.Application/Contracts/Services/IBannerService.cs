using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IBannerService
{
    Task<IReadOnlyList<BannerDoCarrosselDTO>> ListarVigentesDoCarrossel();

    Task<IReadOnlyList<ResumoDeBannerDTO>> ListarParaGestao();

    Task<BannerDTO> BuscarParaEdicao(Guid bannerId);

    Task Cadastrar(BannerDTO dto, string imagemUrl);

    Task Atualizar(BannerDTO dto, string? novaImagemUrl);

    Task Ativar(Guid bannerId);

    Task Desativar(Guid bannerId);
}
