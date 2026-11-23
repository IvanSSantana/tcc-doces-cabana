namespace DocesCabana.Application.DTOs;

public class HorarioDeRetiradaDTO
{
    public DayOfWeek DiaDaSemana { get; set; }

    public bool Aberto { get; set; }

    public TimeOnly? Abertura { get; set; }

    public TimeOnly? Fechamento { get; set; }
}
