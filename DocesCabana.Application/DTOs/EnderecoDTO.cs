namespace DocesCabana.Application.DTOs;

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
