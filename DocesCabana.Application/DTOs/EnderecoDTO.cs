namespace DocesCabana.Application.DTOs;

// Usado na listagem e nos dois formulários (cadastro e edição, spec 018).
// Sem UsuarioId: quem é o dono nunca vem do formulário, sempre da claim de
// quem está autenticado — RN-05 não pode depender de o cliente informar
// corretamente de quem é o endereço.
public class EnderecoDTO
{
    public Guid EnderecoId { get; set; }
    public string Estado { get; set; } = default!;
    public string Cidade { get; set; } = default!;
    public string Bairro { get; set; } = default!;
    public string CEP { get; set; } = default!;
    public string Rua { get; set; } = default!;
    public int Numero { get; set; }
    public string? Complemento { get; set; }
    public bool Padrao { get; set; }
}
