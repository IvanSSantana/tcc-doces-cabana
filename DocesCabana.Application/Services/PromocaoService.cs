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

public class PromocaoService : IPromocaoService
{
    private readonly IPromocaoRepository _promocaoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly ISubcategoriaRepository _subcategoriaRepository;
    private readonly TimeProvider _relogio;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheDaAplicacao _cache;

    public PromocaoService(
        IPromocaoRepository promocaoRepository, IProdutoRepository produtoRepository,
        ISubcategoriaRepository subcategoriaRepository, TimeProvider relogio, IUnitOfWork unitOfWork,
        ICacheDaAplicacao cache)
    {
        _cache = cache;
        _promocaoRepository = promocaoRepository;
        _produtoRepository = produtoRepository;
        _subcategoriaRepository = subcategoriaRepository;
        _relogio = relogio;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lida em quase toda página (vitrine, catálogo, carrinho). A chave é do dia de Brasília,
    /// então a virada do dia já consulta de novo; escrita de promoção remove a chave de hoje.
    /// </summary>
    public Task<PromocoesVigentes> CarregarVigentes()
    {
        var hoje = HorarioDeBrasilia.Hoje(_relogio);

        return _cache.ObterOuCriar(ChavesDeCache.PromocoesVigentes(hoje), () => ConsultarVigentes(hoje), TimeSpan.FromDays(1));
    }

    private async Task<PromocoesVigentes> ConsultarVigentes(DateOnly hoje)
    {

        var vigentes = (await _promocaoRepository.ListarVigentes(hoje))
            .Where(p => p.EstaVigente(hoje))
            .ToList();

        if (vigentes.Count == 0)
            return PromocoesVigentes.Nenhuma;

        var subcategorias = await _subcategoriaRepository.BuscarTodos();
        var categoriaPorSubcategoria = subcategorias.ToDictionary(s => s.SubcategoriaId, s => s.CategoriaId);

        return new PromocoesVigentes(vigentes, categoriaPorSubcategoria);
    }

    public async Task<IReadOnlyList<ResumoDePromocaoDTO>> ListarParaGestao()
    {
        var hoje = HorarioDeBrasilia.Hoje(_relogio);
        var promocoes = await _promocaoRepository.ListarParaGestao();

        return promocoes
            .Select(p => PromocaoMapper.ToResumoDTO(p, hoje))
            .ToList();
    }

    public async Task<PromocaoDTO> BuscarParaEdicao(Guid promocaoId)
    {
        var promocao = await _promocaoRepository.BuscarComProdutos(promocaoId)
            ?? throw new KeyNotFoundException($"Promoção com ID {promocaoId} não encontrada.");

        return PromocaoMapper.ToDTO(promocao);
    }

    public async Task Cadastrar(PromocaoDTO dto)
    {
        var (categoriaId, subcategoriaId, produtos) = await ResolverAbrangencia(dto);

        var promocao = new Promocao(
            dto.Nome,
            dto.Tipo,
            dto.Valor ?? 0,
            dto.QuantidadeLeve,
            dto.QuantidadePague,
            dto.DataInicio!.Value,
            dto.DataFim!.Value,
            categoriaId,
            subcategoriaId,
            produtos,
            dto.Descricao);

        await _promocaoRepository.Adicionar(promocao);
        await SalvarEEsquecerVigentes();
    }

    public async Task Atualizar(PromocaoDTO dto)
    {
        var promocao = await _promocaoRepository.BuscarComProdutos(dto.PromocaoId)
            ?? throw new KeyNotFoundException($"Promoção com ID {dto.PromocaoId} não encontrada.");

        var (categoriaId, subcategoriaId, produtos) = await ResolverAbrangencia(dto);

        promocao.AlterarDados(dto.Nome, dto.Descricao, dto.Tipo, dto.Valor ?? 0, dto.QuantidadeLeve, dto.QuantidadePague);
        promocao.AlterarPeriodo(dto.DataInicio!.Value, dto.DataFim!.Value);
        promocao.AlterarAbrangencia(categoriaId, subcategoriaId, produtos);

        await SalvarEEsquecerVigentes();
    }

    public async Task Ativar(Guid promocaoId)
    {
        var promocao = await BuscarObrigatoria(promocaoId);

        promocao.Ativar();

        await SalvarEEsquecerVigentes();
    }

    public async Task Desativar(Guid promocaoId)
    {
        var promocao = await BuscarObrigatoria(promocaoId);

        promocao.Desativar();

        await SalvarEEsquecerVigentes();
    }

    private async Task SalvarEEsquecerVigentes()
    {
        await _unitOfWork.SalvarAlteracoes();
        _cache.Remover(ChavesDeCache.PromocoesVigentes(HorarioDeBrasilia.Hoje(_relogio)));
    }

    private async Task<Promocao> BuscarObrigatoria(Guid promocaoId) =>
        await _promocaoRepository.BuscarPorId(promocaoId)
            ?? throw new KeyNotFoundException($"Promoção com ID {promocaoId} não encontrada.");

    private async Task<(Guid? CategoriaId, Guid? SubcategoriaId, List<Produto>? Produtos)> ResolverAbrangencia(PromocaoDTO dto)
    {
        switch (dto.Abrangencia)
        {
            case AbrangenciaDaPromocao.Categoria:
                return (dto.CategoriaId, null, null);

            case AbrangenciaDaPromocao.Subcategoria:
                return (null, dto.SubcategoriaId, null);

            default:
                var ids = dto.ProdutoIds.Distinct().ToList();
                var produtos = await _produtoRepository.BuscarPorIds(ids);

                if (produtos.Count != ids.Count)
                    throw new KeyNotFoundException("Um dos produtos escolhidos não foi encontrado.");

                return (null, null, produtos);
        }
    }
}
