using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class AgendaDeRetiradaDTOValidatorTests
{
    private readonly AgendaDeRetiradaDTOValidator _validator = new();

    private static AgendaDeRetiradaDTO CriarValida() => new()
    {
        DuracaoDaFaixaEmMinutos = 60,
        PreparoEmMinutos = 60,
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
    public void Dado_DuracaoInvalida_Quando_Validar_Entao_DeveSerInvalida(int? duracao)
    {
        var dto = CriarValida();
        dto.DuracaoDaFaixaEmMinutos = duracao;

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.DuracaoDaFaixaEmMinutos));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(240)]
    public void Dado_DuracaoNoLimite_Quando_Validar_Entao_DeveSerValida(int duracao)
    {
        var dto = CriarValida();
        dto.DuracaoDaFaixaEmMinutos = duracao;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(481)]
    public void Dado_PreparoInvalido_Quando_Validar_Entao_DeveSerInvalido(int? preparo)
    {
        var dto = CriarValida();
        dto.PreparoEmMinutos = preparo;

        AssertInvalido(dto, nameof(AgendaDeRetiradaDTO.PreparoEmMinutos));
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
