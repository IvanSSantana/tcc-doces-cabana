using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IPainelDeVendasService
{
    /// <summary>Monta o painel do período que contém <paramref name="referencia"/> (hoje, em Brasília, se nula).</summary>
    Task<PainelDeVendasDTO> Montar(PeriodoDoPainel periodo, DateOnly? referencia);
}
