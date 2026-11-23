using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.Models;

/// <summary>
/// Tela única da retirada no admin: formulário dos horários (prefixo "Agenda") e formulário de
/// data sem retirada (prefixo "NovoBloqueio").
/// </summary>
public class AgendaDeRetiradaViewModel
{
    public AgendaDeRetiradaDTO Agenda { get; init; } = new();

    public DiaSemRetiradaDTO NovoBloqueio { get; init; } = new();
}
