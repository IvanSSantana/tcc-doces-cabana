using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;

namespace DocesCabana.Application.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AvaliacaoService(IAvaliacaoRepository avaliacaoRepository, IUnitOfWork unitOfWork)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResumoAvaliacoesDTO> ResumirPorProduto(Guid produtoId)
    {
        var total = await _avaliacaoRepository.ContarPorProduto(produtoId);
        var distribuicaoBruta = await _avaliacaoRepository.ContarPorNota(produtoId);

        // RN-04: as cinco chaves sempre presentes, mesmo quando o repositório
        // só devolve as notas que de fato têm avaliação.
        var distribuicao = new Dictionary<byte, int>();
        for (byte nota = 1; nota <= 5; nota++)
            distribuicao[nota] = distribuicaoBruta.GetValueOrDefault(nota, 0);

        // RN-03: sem avaliação não tem média — não é zero.
        decimal? media = null;
        if (total > 0)
        {
            var soma = distribuicao.Sum(kv => kv.Key * kv.Value);
            media = Math.Round((decimal)soma / total, 1, MidpointRounding.AwayFromZero);
        }

        return new ResumoAvaliacoesDTO
        {
            Media = media,
            Total = total,
            Distribuicao = distribuicao
        };
    }

    public async Task<PaginaAvaliacoesDTO> ListarPorProduto(
        Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade, Guid? usuarioAtual)
    {
        var total = await _avaliacaoRepository.ContarPorProduto(produtoId);
        var avaliacoes = await _avaliacaoRepository.BuscarPorProduto(produtoId, ordenacao, quantidade);

        var itens = AvaliacaoMapper.ToDTO(avaliacoes, usuarioAtual);

        return new PaginaAvaliacoesDTO
        {
            Itens = itens,
            Ordenacao = ordenacao,
            Exibindo = itens.Count,
            Total = total,
            // RF-15: só quando ainda sobra avaliação fora da página atual.
            TemMais = itens.Count < total
        };
    }

    public async Task<Guid> AlternarVotoUtil(Guid avaliacaoId, Guid usuarioId)
    {
        var avaliacao = await _avaliacaoRepository.BuscarComVotos(avaliacaoId);
        if (avaliacao is null)
            throw new KeyNotFoundException($"Avaliação com ID {avaliacaoId} não encontrada.");

        // RN-06/RN-07 vivem na entidade — lança InvalidOperationException se
        // usuarioId for o autor. Nada é persistido nesse caso.
        avaliacao.AlternarVotoUtil(usuarioId);

        _avaliacaoRepository.Atualizar(avaliacao);
        await _unitOfWork.SalvarAlteracoes();

        return avaliacao.ProdutoId;
    }
}
