using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Services;

public class BannerService : IBannerService
{
    /// <summary>
    /// A home lê os banners a cada visita. Toda escrita de banner (e de produto, que pode tirar
    /// um banner do ar) remove a chave; a expiração curta cobre a virada do dia.
    /// </summary>
    private static readonly TimeSpan DuracaoDoCache = TimeSpan.FromMinutes(10);

    private readonly IBannerRepository _bannerRepository;
    private readonly TimeProvider _relogio;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheDaAplicacao _cache;

    public BannerService(
        IBannerRepository bannerRepository, TimeProvider relogio, IUnitOfWork unitOfWork, ICacheDaAplicacao cache)
    {
        _bannerRepository = bannerRepository;
        _relogio = relogio;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public Task<IReadOnlyList<BannerDoCarrosselDTO>> ListarVigentesDoCarrossel() =>
        _cache.ObterOuCriar(ChavesDeCache.BannersVigentes, ConsultarVigentes, DuracaoDoCache);

    public async Task<IReadOnlyList<ResumoDeBannerDTO>> ListarParaGestao()
    {
        var hoje = HorarioDeBrasilia.Hoje(_relogio);
        var banners = await _bannerRepository.ListarParaGestao();

        return banners
            .Select(b => BannerMapper.ToResumoDTO(b, hoje))
            .ToList();
    }

    public async Task<BannerDTO> BuscarParaEdicao(Guid bannerId) =>
        BannerMapper.ToDTO(await BuscarObrigatorio(bannerId));

    public async Task Cadastrar(BannerDTO dto, string imagemUrl)
    {
        var (categoriaId, subcategoriaId, produtoId) = ResolverDestino(dto);

        var banner = new Banner(
            imagemUrl,
            dto.TextoAlternativo,
            dto.Ordem!.Value,
            dto.DataInicio!.Value,
            dto.DataFim,
            categoriaId,
            subcategoriaId,
            produtoId);

        await _bannerRepository.Adicionar(banner);
        await SalvarEEsquecerVigentes();
    }

    public async Task Atualizar(BannerDTO dto, string? novaImagemUrl)
    {
        var banner = await BuscarObrigatorio(dto.BannerId);
        var (categoriaId, subcategoriaId, produtoId) = ResolverDestino(dto);

        if (novaImagemUrl is not null)
            banner.AlterarImagem(novaImagemUrl);

        banner.AlterarApresentacao(dto.TextoAlternativo, dto.Ordem!.Value);
        banner.AlterarPeriodo(dto.DataInicio!.Value, dto.DataFim);
        banner.AlterarDestino(categoriaId, subcategoriaId, produtoId);

        await SalvarEEsquecerVigentes();
    }

    public async Task Ativar(Guid bannerId)
    {
        var banner = await BuscarObrigatorio(bannerId);

        banner.Ativar();

        await SalvarEEsquecerVigentes();
    }

    public async Task Desativar(Guid bannerId)
    {
        var banner = await BuscarObrigatorio(bannerId);

        banner.Desativar();

        await SalvarEEsquecerVigentes();
    }

    private async Task<IReadOnlyList<BannerDoCarrosselDTO>> ConsultarVigentes()
    {
        var banners = await _bannerRepository.ListarVigentes(HorarioDeBrasilia.Hoje(_relogio));

        return banners
            .Select(BannerMapper.ToCarrosselDTO)
            .ToList();
    }

    private async Task<Banner> BuscarObrigatorio(Guid bannerId) =>
        await _bannerRepository.BuscarPorId(bannerId)
            ?? throw new KeyNotFoundException($"Banner com ID {bannerId} não encontrado.");

    /// <summary>
    /// O formulário pode trazer os três seletores preenchidos; vale só o do tipo escolhido.
    /// </summary>
    private static (Guid? CategoriaId, Guid? SubcategoriaId, Guid? ProdutoId) ResolverDestino(BannerDTO dto) =>
        dto.TipoDeDestino switch
        {
            TipoDeDestinoDoBanner.Categoria => (dto.CategoriaId, null, null),
            TipoDeDestinoDoBanner.Subcategoria => (null, dto.SubcategoriaId, null),
            _ => (null, null, dto.ProdutoId),
        };

    private async Task SalvarEEsquecerVigentes()
    {
        await _unitOfWork.SalvarAlteracoes();
        _cache.Remover(ChavesDeCache.BannersVigentes);
    }
}
