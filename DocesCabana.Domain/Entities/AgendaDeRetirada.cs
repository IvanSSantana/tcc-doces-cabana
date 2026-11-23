using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Domain.Entities;

/// <summary>
/// Configuração única da retirada na loja: horário de cada dia da semana, duração das faixas,
/// tempo de preparo e datas sem retirada. Gera as faixas que o cliente pode escolher no dia.
/// </summary>
public class AgendaDeRetirada
{
    public static readonly Guid IdUnico = Guid.Parse("5b8e2f4a-1c3d-4e6f-9a0b-7c2d1e3f4a5b");

    public const int DuracaoMinima = 15;
    public const int DuracaoMaxima = 240;
    public const int PreparoMaximo = 480;
    public const int DiasDeBuscaDaProximaAbertura = 14;

    public Guid AgendaDeRetiradaId { get; private set; }

    public int DuracaoDaFaixaEmMinutos { get; private set; }

    public int PreparoEmMinutos { get; private set; }

    private readonly List<HorarioDeRetirada> _horarios = [];

    public IReadOnlyCollection<HorarioDeRetirada> Horarios => _horarios.AsReadOnly();

    private readonly List<DiaSemRetirada> _diasBloqueados = [];

    public IReadOnlyCollection<DiaSemRetirada> DiasBloqueados => _diasBloqueados.AsReadOnly();

    protected AgendaDeRetirada() { }

    public AgendaDeRetirada(int duracaoDaFaixaEmMinutos, int preparoEmMinutos, Guid id = default)
    {
        AlterarFaixas(duracaoDaFaixaEmMinutos, preparoEmMinutos);

        AgendaDeRetiradaId = id == Guid.Empty
            ? IdUnico
            : id;

        foreach (var dia in Enum.GetValues<DayOfWeek>())
            _horarios.Add(new HorarioDeRetirada(dia, null, null));
    }

    public void AlterarFaixas(int duracaoDaFaixaEmMinutos, int preparoEmMinutos)
    {
        if (duracaoDaFaixaEmMinutos < DuracaoMinima || duracaoDaFaixaEmMinutos > DuracaoMaxima)
            throw new ArgumentException(
                $"A duração da faixa precisa ficar entre {DuracaoMinima} e {DuracaoMaxima} minutos.", nameof(duracaoDaFaixaEmMinutos));

        if (preparoEmMinutos < 0 || preparoEmMinutos > PreparoMaximo)
            throw new ArgumentException(
                $"O tempo de preparo precisa ficar entre 0 e {PreparoMaximo} minutos.", nameof(preparoEmMinutos));

        DuracaoDaFaixaEmMinutos = duracaoDaFaixaEmMinutos;
        PreparoEmMinutos = preparoEmMinutos;
    }

    /// <summary>Abertura e fechamento nulos deixam o dia fechado.</summary>
    public void DefinirHorario(DayOfWeek dia, TimeOnly? abertura, TimeOnly? fechamento) =>
        _horarios.Single(h => h.DiaDaSemana == dia).Alterar(abertura, fechamento);

    public void BloquearDia(DateOnly data, string? motivo, DateOnly hoje)
    {
        if (data < hoje)
            throw new ArgumentException("Não é possível bloquear uma data que já passou.", nameof(data));

        if (EstaBloqueado(data))
            throw new InvalidOperationException("Esta data já está bloqueada.");

        _diasBloqueados.Add(new DiaSemRetirada(data, motivo));
    }

    public void DesbloquearDia(DateOnly data) =>
        _diasBloqueados.RemoveAll(d => d.Data == data);

    /// <summary>
    /// Faixas do dia de <paramref name="agoraLocal"/>: começam na abertura, de duração em duração,
    /// e a última termina no fechamento. Só entram as que começam depois de agora + preparo.
    /// </summary>
    public IReadOnlyList<FaixaDeRetirada> FaixasDisponiveis(DateTime agoraLocal)
    {
        var data = DateOnly.FromDateTime(agoraLocal);
        var horario = HorarioDoDia(data);

        if (horario is null)
            return [];

        var limite = agoraLocal.AddMinutes(PreparoEmMinutos);
        var fechamento = data.ToDateTime(horario.Fechamento!.Value);
        var faixas = new List<FaixaDeRetirada>();

        for (var inicio = data.ToDateTime(horario.Abertura!.Value); inicio < fechamento; inicio = inicio.AddMinutes(DuracaoDaFaixaEmMinutos))
        {
            if (inicio < limite)
                continue;

            var fim = inicio.AddMinutes(DuracaoDaFaixaEmMinutos);
            if (fim > fechamento)
                fim = fechamento;

            faixas.Add(new FaixaDeRetirada(data, TimeOnly.FromDateTime(inicio), TimeOnly.FromDateTime(fim)));
        }

        return faixas;
    }

    /// <summary>Abertura do primeiro dia seguinte aberto e não bloqueado, até 14 dias à frente.</summary>
    public DateTime? ProximaAbertura(DateTime agoraLocal)
    {
        var hoje = DateOnly.FromDateTime(agoraLocal);

        for (var i = 1; i <= DiasDeBuscaDaProximaAbertura; i++)
        {
            var data = hoje.AddDays(i);
            var horario = HorarioDoDia(data);

            if (horario is not null)
                return data.ToDateTime(horario.Abertura!.Value);
        }

        return null;
    }

    private HorarioDeRetirada? HorarioDoDia(DateOnly data)
    {
        if (EstaBloqueado(data))
            return null;

        var horario = _horarios.Single(h => h.DiaDaSemana == data.DayOfWeek);
        return horario.Aberto ? horario : null;
    }

    private bool EstaBloqueado(DateOnly data) => _diasBloqueados.Any(d => d.Data == data);
}
