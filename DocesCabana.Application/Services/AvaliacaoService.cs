using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AvaliacaoService(
        IAvaliacaoRepository avaliacaoRepository, IPedidoRepository pedidoRepository, IUnitOfWork unitOfWork)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResumoAvaliacoesDTO> ResumirPorProduto(Guid produtoId)
    {
        var total = await _avaliacaoRepository.ContarPorProduto(produtoId);
        var distribuicaoBruta = await _avaliacaoRepository.ContarPorNota(produtoId);

        var distribuicao = new Dictionary<byte, int>();
        for (byte nota = 1; nota <= 5; nota++)
            distribuicao[nota] = distribuicaoBruta.GetValueOrDefault(nota, 0);

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
            TemMais = itens.Count < total
        };
    }

    public async Task<Guid> AlternarVotoUtil(Guid avaliacaoId, Guid usuarioId)
    {
        var avaliacao = await _avaliacaoRepository.BuscarComVotos(avaliacaoId);
        if (avaliacao is null)
            throw new KeyNotFoundException($"Avaliação com ID {avaliacaoId} não encontrada.");

        avaliacao.AlternarVotoUtil(usuarioId);

        await _avaliacaoRepository.Atualizar(avaliacao);
        await _unitOfWork.SalvarAlteracoes();

        return avaliacao.ProdutoId;
    }

    public async Task<SituacaoDaAvaliacao> VerificarSituacao(Guid produtoId, Guid usuarioId)
    {
        if (await _avaliacaoRepository.ExistePorUsuarioEProduto(usuarioId, produtoId))
            return SituacaoDaAvaliacao.JaAvaliou;

        return await _pedidoRepository.ExisteEntregueComProduto(usuarioId, produtoId)
            ? SituacaoDaAvaliacao.PodeAvaliar
            : SituacaoDaAvaliacao.NaoRecebeu;
    }

    public async Task Avaliar(NovaAvaliacaoDTO dto, Guid usuarioId)
    {
        var situacao = await VerificarSituacao(dto.ProdutoId, usuarioId);

        switch (situacao)
        {
            case SituacaoDaAvaliacao.JaAvaliou:
                throw new InvalidOperationException("Você já comentou sobre esse produto.");
            case SituacaoDaAvaliacao.NaoRecebeu:
                throw new InvalidOperationException("Só quem recebeu este produto pode avaliá-lo.");
        }

        var nota = dto.Nota
            ?? throw new ArgumentException("Escolha uma nota de 1 a 5.", nameof(dto));

        var comentario = string.IsNullOrWhiteSpace(dto.Comentario)
            ? null
            : dto.Comentario.Trim();

        var avaliacao = new Avaliacao(usuarioId, dto.ProdutoId, nota, comentario);

        await _avaliacaoRepository.Adicionar(avaliacao);
        await _unitOfWork.SalvarAlteracoes();
    }
}
