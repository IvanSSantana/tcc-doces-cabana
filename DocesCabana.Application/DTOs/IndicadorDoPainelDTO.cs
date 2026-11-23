namespace DocesCabana.Application.DTOs;

/// <summary>Valor do período e do período anterior. Variação nula quando o anterior é zero.</summary>
public record IndicadorDoPainelDTO(decimal Atual, decimal Anterior, decimal? VariacaoPercentual);
