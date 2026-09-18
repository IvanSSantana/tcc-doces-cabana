using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DocesCabana.Application.DTOs;

public class DadosPessoaisDTO
{
    public string Nome { get; set; } = default!;
    public string Celular { get; set; } = default!;

    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime DataNascimento { get; set; }

    [ValidateNever]
    public string CPF { get; set; } = default!;
}
