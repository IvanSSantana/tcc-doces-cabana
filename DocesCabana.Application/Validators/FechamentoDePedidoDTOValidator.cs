using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Enums;
using FluentValidation;

namespace DocesCabana.Application.Validators;

public class FechamentoDePedidoDTOValidator : AbstractValidator<FechamentoDePedidoDTO>
{
    public FechamentoDePedidoDTOValidator()
    {
        RuleFor(x => x.Modalidade)
            .IsInEnum().WithMessage("Escolha como quer receber o pedido!");

        RuleFor(x => x.EnderecoId)
            .NotEmpty().WithMessage("Escolha um endereço de entrega!")
            .When(x => x.Modalidade == ModalidadeDeEntrega.Entrega);

        RuleFor(x => x.ServicoDeEntregaId)
            .GreaterThan(0).WithMessage("Escolha uma opção de entrega!")
            .When(x => x.Modalidade == ModalidadeDeEntrega.Entrega);

        RuleFor(x => x.InicioDaFaixa)
            .NotNull().WithMessage("Escolha uma faixa de horário!")
            .When(x => x.Modalidade == ModalidadeDeEntrega.RetiradaNaLoja);

        RuleFor(x => x.MetodoPagamento)
            .IsInEnum().WithMessage("Escolha uma forma de pagamento!");
    }
}
