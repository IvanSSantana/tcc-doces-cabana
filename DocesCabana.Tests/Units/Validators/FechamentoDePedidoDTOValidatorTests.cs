using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Validators;

public class FechamentoDePedidoDTOValidatorTests
{
    private readonly FechamentoDePedidoDTOValidator _validator = new();

    private static FechamentoDePedidoDTO CriarEntrega() => new()
    {
        Modalidade = ModalidadeDeEntrega.Entrega,
        EnderecoId = Guid.NewGuid(),
        ServicoDeEntregaId = 1,
        MetodoPagamento = MetodoPagamento.Pix
    };

    private static FechamentoDePedidoDTO CriarRetirada() => new()
    {
        Modalidade = ModalidadeDeEntrega.RetiradaNaLoja,
        InicioDaFaixa = new TimeOnly(14, 0),
        MetodoPagamento = MetodoPagamento.Pix
    };

    private void AssertInvalido(FechamentoDePedidoDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_EntregaValida_Quando_Validar_Entao_DevePassar()
    {
        Assert.True(_validator.Validate(CriarEntrega()).IsValid);
    }

    [Fact]
    public void Dado_EntregaSemEndereco_Quando_Validar_Entao_DeveFalhar()
    {
        var dto = CriarEntrega();
        dto.EnderecoId = Guid.Empty;

        AssertInvalido(dto, nameof(FechamentoDePedidoDTO.EnderecoId));
    }

    [Fact]
    public void Dado_EntregaSemServico_Quando_Validar_Entao_DeveFalhar()
    {
        var dto = CriarEntrega();
        dto.ServicoDeEntregaId = 0;

        AssertInvalido(dto, nameof(FechamentoDePedidoDTO.ServicoDeEntregaId));
    }

    [Fact]
    public void Dado_RetiradaSemEndereco_Quando_Validar_Entao_DevePassar()
    {
        Assert.True(_validator.Validate(CriarRetirada()).IsValid);
    }

    [Fact]
    public void Dado_RetiradaSemFaixa_Quando_Validar_Entao_DeveFalhar()
    {
        var dto = CriarRetirada();
        dto.InicioDaFaixa = null;

        AssertInvalido(dto, nameof(FechamentoDePedidoDTO.InicioDaFaixa));
    }

    [Fact]
    public void Dado_ModalidadeInvalida_Quando_Validar_Entao_DeveFalhar()
    {
        var dto = CriarEntrega();
        dto.Modalidade = (ModalidadeDeEntrega)9;

        AssertInvalido(dto, nameof(FechamentoDePedidoDTO.Modalidade));
    }

    [Fact]
    public void Dado_MetodoDePagamentoInvalido_Quando_Validar_Entao_DeveFalhar()
    {
        var dto = CriarRetirada();
        dto.MetodoPagamento = (MetodoPagamento)99;

        AssertInvalido(dto, nameof(FechamentoDePedidoDTO.MetodoPagamento));
    }
}
