using DocesCabana.Domain.ValueObjects;

namespace DocesCabana.Domain.Entities;

/// <summary>
/// Configuração única da retirada na loja: horário de cada dia da semana, tempo de preparo
/// (que também é a duração de cada faixa), intervalo de almoço opcional e datas sem retirada.
/// Gera as faixas que o cliente pode escolher no dia.
/// </summary>
public class AgendaDeRetirada
{
    public static readonly Guid IdUnico = Guid.Parse("5b8e2f4a-1c3d-4e6f-9a0b-7c2d1e3f4a5b");

    public const int PreparoMinimo = 15;
    public const int PreparoMaximo = 240;
    public const int DiasDeBuscaDaProximaAbertura = 14;

    public Guid AgendaDeRetiradaId { get; private set; }

    /// <summary>Antecedência mínima do pedido e, ao mesmo tempo, duração de cada faixa.</summary>
    public int TempoDePreparoEmMinutos { get; private set; }

    /// <summary>Início do almoço; nulo junto com o fim quando não há intervalo.</summary>
    public TimeOnly? InicioDoAlmoco { get; private set; }

    public TimeOnly? FimDoAlmoco { get; private set; }

    public bool TemAlmoco => InicioDoAlmoco is not null && FimDoAlmoco is not null;

    private readonly List<HorarioDeRetirada> _horarios = [];

    public IReadOnlyCollection<HorarioDeRetirada> Horarios => _horarios.AsReadOnly();

    private readonly List<DiaSemRetirada> _diasBloqueados = [];

    public IReadOnlyCollection<DiaSemRetirada> DiasBloqueados => _diasBloqueados.AsReadOnly();

    protected AgendaDeRetirada() { }

    public AgendaDeRetirada(int tempoDePreparoEmMinutos, Guid id = default)
    {
        AlterarTempoDePreparo(tempoDePreparoEmMinutos);

        AgendaDeRetiradaId = id == Guid.Empty
            ? IdUnico
            : id;

        foreach (var dia in Enum.GetValues<DayOfWeek>())
            _horarios.Add(new HorarioDeRetirada(dia, null, null));
    }

    public void AlterarTempoDePreparo(int tempoDePreparoEmMinutos)
    {
        if (tempoDePreparoEmMinutos < PreparoMinimo || tempoDePreparoEmMinutos > PreparoMaximo)
            throw new ArgumentException(
                $"O tempo de preparo precisa ficar entre {PreparoMinimo} e {PreparoMaximo} minutos.", nameof(tempoDePreparoEmMinutos));

        TempoDePreparoEmMinutos = tempoDePreparoEmMinutos;
    }

    /// <summary>Início e fim nulos removem o intervalo de almoço.</summary>
    public void DefinirAlmoco(TimeOnly? inicio, TimeOnly? fim)
    {
        if (inicio is null != fim is null)
            throw new ArgumentException("Informe o início e o fim do almoço, ou deixe sem almoço.", nameof(inicio));

        if (inicio is not null && inicio >= fim)
            throw new ArgumentException("O início do almoço precisa ser antes do fim.", nameof(inicio));

        InicioDoAlmoco = inicio;
        FimDoAlmoco = fim;
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
    /// Faixas do dia de <paramref name="agoraLocal"/>: começam na abertura, de preparo em preparo,
    /// e a última termina no fechamento. O almoço divide o dia em dois turnos: a faixa que encostaria
    /// nele termina no início do almoço e a contagem recomeça no fim dele.
    /// Só entram as faixas que começam depois de agora + preparo.
    /// </summary>
    public IReadOnlyList<FaixaDeRetirada> FaixasDisponiveis(DateTime agoraLocal)
    {
        var data = DateOnly.FromDateTime(agoraLocal);
        var horario = HorarioDoDia(data);

        if (horario is null)
            return [];

        var limite = agoraLocal.AddMinutes(TempoDePreparoEmMinutos);
        var faixas = new List<FaixaDeRetirada>();

        foreach (var (inicioDoTurno, fimDoTurno) in Turnos(horario.Abertura!.Value, horario.Fechamento!.Value))
        {
            var fechamento = data.ToDateTime(fimDoTurno);

            for (var inicio = data.ToDateTime(inicioDoTurno); inicio < fechamento; inicio = inicio.AddMinutes(TempoDePreparoEmMinutos))
            {
                if (inicio < limite)
                    continue;

                var fim = inicio.AddMinutes(TempoDePreparoEmMinutos);
                if (fim > fechamento)
                    fim = fechamento;

                faixas.Add(new FaixaDeRetirada(data, TimeOnly.FromDateTime(inicio), TimeOnly.FromDateTime(fim)));
            }
        }

        return faixas;
    }

    /// <summary>Períodos de atendimento do dia, descontado o almoço que cair dentro do horário.</summary>
    private IEnumerable<(TimeOnly Inicio, TimeOnly Fim)> Turnos(TimeOnly abertura, TimeOnly fechamento)
    {
        if (!TemAlmoco || InicioDoAlmoco >= fechamento || FimDoAlmoco <= abertura)
        {
            yield return (abertura, fechamento);
            yield break;
        }

        if (InicioDoAlmoco!.Value > abertura)
            yield return (abertura, InicioDoAlmoco.Value);

        if (FimDoAlmoco!.Value < fechamento)
            yield return (FimDoAlmoco.Value, fechamento);
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
