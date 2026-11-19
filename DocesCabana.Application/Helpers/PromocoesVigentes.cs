using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Helpers;

public sealed class PromocoesVigentes
{
    private readonly IReadOnlyList<Promocao> _vigentes;
    private readonly IReadOnlyDictionary<Guid, Guid> _categoriaPorSubcategoria;

    public static PromocoesVigentes Nenhuma { get; } = new([], new Dictionary<Guid, Guid>());

    public PromocoesVigentes(IEnumerable<Promocao> vigentes, IReadOnlyDictionary<Guid, Guid> categoriaPorSubcategoria)
    {
        _vigentes = vigentes.ToList();
        _categoriaPorSubcategoria = categoriaPorSubcategoria;
    }

    public DescontoAplicadoDTO? MelhorPara(Guid produtoId, Guid subcategoriaId, decimal precoUnitario, short quantidade)
    {
        var valorBruto = precoUnitario * quantidade;

        return QueAlcancam(produtoId, subcategoriaId)
            .Select(p => (Promocao: p, Desconto: p.CalcularDesconto(precoUnitario, quantidade)))
            .Where(c => c.Desconto > 0 && c.Desconto < valorBruto)
            .OrderByDescending(c => c.Desconto)
            .Select(c => new DescontoAplicadoDTO(c.Desconto, PromocaoMapper.Rotulo(c.Promocao)))
            .FirstOrDefault();
    }

    public (decimal? PrecoPromocional, IReadOnlyList<string> Selos) ParaVitrine(Guid produtoId, Guid subcategoriaId, decimal preco)
    {
        var alcancam = QueAlcancam(produtoId, subcategoriaId).ToList();
        var selos = new List<string>();

        var melhorPorUnidade = MelhorPara(produtoId, subcategoriaId, preco, 1);
        decimal? precoPromocional = null;

        if (melhorPorUnidade is not null)
        {
            precoPromocional = preco - melhorPorUnidade.Valor;
            selos.Add(melhorPorUnidade.Rotulo);
        }

        selos.AddRange(alcancam
            .Where(p => p.Tipo == TipoDeDesconto.CompreXLeveY)
            .Select(PromocaoMapper.Rotulo)
            .Distinct());

        return (precoPromocional, selos);
    }

    private IEnumerable<Promocao> QueAlcancam(Guid produtoId, Guid subcategoriaId)
    {
        var categoriaId = _categoriaPorSubcategoria.TryGetValue(subcategoriaId, out var encontrada)
            ? encontrada
            : Guid.Empty;

        return _vigentes.Where(p => p.Alcanca(produtoId, subcategoriaId, categoriaId));
    }
}
