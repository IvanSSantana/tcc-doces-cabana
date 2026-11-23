namespace DocesCabana.Application.DTOs;

public class AgendaDeRetiradaDTO
{
    public int? DuracaoDaFaixaEmMinutos { get; set; }

    public int? PreparoEmMinutos { get; set; }

    /// <summary>Um item por dia da semana, de segunda a domingo.</summary>
    public List<HorarioDeRetiradaDTO> Horarios { get; set; } = [];

    /// <summary>Só leitura na tela: as datas são incluídas e removidas por formulários próprios.</summary>
    public List<DiaSemRetiradaDTO> DiasBloqueados { get; set; } = [];
}
