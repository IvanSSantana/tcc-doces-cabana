using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class AgendaDeRetiradaDTOValidatorTests
{
    private readonly AgendaDeRetiradaDTOValidator _validator = new();

    private static AgendaDeRetiradaDTO CriarValida() => new()
    {
        TempoDePreparoEmMinutos = 60,
        Horarios = Enum.GetValues<DayOfWeek>()
            .Select(dia => dia == DayOfWeek.Sunday
                ? new HorarioDeRetiradaDTO { DiaDaSemana = dia, Aberto = false }
                : new HorarioDeRetiradaDTO { DiaDaSemana = dia, Aberto = true, Abertura = new TimeOnly(9, 0), Fechamento = new TimeOnly(18, 0) })
            .ToList()
    };

    private void AssertInvalido(AgendaDeRetiradaDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_AgendaValida_Quando_Validar_Entao_DeveSerValida()
    {
        Assert.True(_validator.Validate(CriarValida()).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(14)]
    [InlineData(241)]
    public void Dado_PreparoInvalido_Quando_Validar_Entao_DeveSerInvalido(int? preparo)
    {
        var dto = CriarValida();
        dto.TempoDePreparoEmMinutos = preparo;

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.TempoDePreparoEmMinutos));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(240)]
    public void Dado_PreparoNoLimite_Quando_Validar_Entao_DeveSerValido(int preparo)
    {
        var dto = CriarValida();
        dto.TempoDePreparoEmMinutos = preparo;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_AlmocoValido_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.TemAlmoco = true;
        dto.InicioDoAlmoco = new TimeOnly(12, 0);
        dto.FimDoAlmoco = new TimeOnly(13, 0);

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_AlmocoSemInicio_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.TemAlmoco = true;
        dto.FimDoAlmoco = new TimeOnly(13, 0);

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.InicioDoAlmoco));
    }

    [Fact]
    public void Dado_AlmocoComFimAntesDoInicio_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.TemAlmoco = true;
        dto.InicioDoAlmoco = new TimeOnly(13, 0);
        dto.FimDoAlmoco = new TimeOnly(12, 0);

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.FimDoAlmoco));
    }

    [Fact]
    public void Dado_SemAlmocoComHorariosVazios_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.TemAlmoco = false;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_SeisDias_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Horarios.RemoveAt(0);

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.Horarios));
    }

    [Fact]
    public void Dado_DiaRepetido_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Horarios[0].DiaDaSemana = dto.Horarios[1].DiaDaSemana;

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.Horarios));
    }

    [Fact]
    public void Dado_DiaAbertoSemAbertura_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Horarios[1].Abertura = null;

        AssertInvalido(dto, "Horarios[1].Abertura");
    }

    [Fact]
    public void Dado_DiaAbertoSemFechamento_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Horarios[1].Fechamento = null;

        AssertInvalido(dto, "Horarios[1].Fechamento");
    }

    [Fact]
    public void Dado_AberturaDepoisDoFechamento_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Horarios[1].Abertura = new TimeOnly(18, 0);
        dto.Horarios[1].Fechamento = new TimeOnly(9, 0);

        AssertInvalido(dto, "Horarios[1].Fechamento");
    }

    [Fact]
    public void Dado_DiaFechadoSemHorarios_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.Horarios[1] = new HorarioDeRetiradaDTO { DiaDaSemana = dto.Horarios[1].DiaDaSemana, Aberto = false };

        Assert.True(_validator.Validate(dto).IsValid);
    }
}
