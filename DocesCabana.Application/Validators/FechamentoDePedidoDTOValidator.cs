using DocesCabana.Application.DTOs;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class FechamentoDePedidoDTOValidator : AbstractValidator<FechamentoDePedidoDTO>
{
    public FechamentoDePedidoDTOValidator()
    {
        RuleFor(x => x.EnderecoId)
            .NotEmpty().WithMessage("Escolha um endereço de entrega!");

        RuleFor(x => x.ServicoDeEntregaId)
            .GreaterThan(0).WithMessage("Escolha uma opção de entrega!");

        RuleFor(x => x.MetodoPagamento)
            .IsInEnum().WithMessage("Escolha uma forma de pagamento!");
    }
}
