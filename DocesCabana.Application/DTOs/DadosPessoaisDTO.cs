using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DocesCabana.Application.DTOs;

// RF-04 a RF-06 (spec 018): CPF viaja aqui só para exibição — nunca é campo
// de formulário (RN-06 diz que não muda), então nunca vem de input do
// usuário. O controlador o preenche a partir do dado já guardado, inclusive
// ao redesenhar a tela depois de um erro (CA-07).
public class DadosPessoaisDTO
{
    public string Nome { get; set; } = default!;
    public string Celular { get; set; } = default!;

    // Sem isto, o Input Tag Helper renderiza DateTime com o padrão geral da
    // cultura ("06/06/1994 00:00:00", não só a data) — o que o formulário
    // de dados pessoais devolve pré-preenchido, diferente do de cadastro
    // (onde o campo sempre nasce vazio, então o problema nunca apareceu).
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime DataNascimento { get; set; }

    // ValidateNever: nunca é campo de formulário (comentário acima), então
    // nunca vem preenchido de um POST. Sem isto, o ASP.NET Core trata toda
    // propriedade de referência não anulável como implicitamente
    // [Required] — CPF chegaria sempre nulo do form e invalidaria o
    // ModelState em silêncio, sem span nenhum pra mostrar por quê (CPF não
    // é um <input>, então não tem onde `asp-validation-for` desenhar o
    // erro). Achado ao rodar o E2E de CA-05.
    [ValidateNever]
    public string CPF { get; set; } = default!;
}
