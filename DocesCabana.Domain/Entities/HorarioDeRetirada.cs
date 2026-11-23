namespace DocesCabana.Domain.Entities;

public class HorarioDeRetirada
{
    public DayOfWeek DiaDaSemana { get; private set; }

    public TimeOnly? Abertura { get; private set; }

    public TimeOnly? Fechamento { get; private set; }

    public bool Aberto => Abertura is not null && Fechamento is not null;

    protected HorarioDeRetirada() { }

    internal HorarioDeRetirada(DayOfWeek diaDaSemana, TimeOnly? abertura, TimeOnly? fechamento)
    {
        DiaDaSemana = diaDaSemana;
        Alterar(abertura, fechamento);
    }

    internal void Alterar(TimeOnly? abertura, TimeOnly? fechamento)
    {
        if (abertura is null != fechamento is null)
            throw new ArgumentException("Informe a abertura e o fechamento, ou deixe o dia fechado.", nameof(abertura));

        if (abertura is not null && abertura >= fechamento)
            throw new ArgumentException("A abertura precisa ser antes do fechamento.", nameof(abertura));

        Abertura = abertura;
        Fechamento = fechamento;
    }
}
