using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public class Endereco
{
    public Guid EnderecoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Usuario? Usuario { get; private set; }

    public string Estado { get; private set; } = default!;

    public string Cidade { get; private set; } = default!;

    public string Bairro { get; private set; } = default!;

    public string CEP { get; private set; } = default!;

    public string Rua { get; private set; } = default!;

    public int Numero { get; private set; }

    public string? Complemento { get; private set; }

    public bool Padrao { get; private set; }

    public DateTime DataCadastro { get; private set; }

    protected Endereco() { }

    public Endereco(
        Guid usuarioId,
        string estado,
        string cidade,
        string bairro,
        string cep,
        string rua,
        int numero,
        string? complemento = null,
        Guid id = default)
    {
        ValidarUsuario(usuarioId);
        ValidarObrigatorio(estado, nameof(estado));
        ValidarObrigatorio(cidade, nameof(cidade));
        ValidarObrigatorio(bairro, nameof(bairro));
        ValidarCep(cep);
        ValidarObrigatorio(rua, nameof(rua));
        ValidarNumero(numero);

        EnderecoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        UsuarioId = usuarioId;
        Estado = estado;
        Cidade = cidade;
        Bairro = bairro;
        CEP = CepHelper.ApenasDigitos(cep);
        Rua = rua;
        Numero = numero;
        Complemento = complemento;
        DataCadastro = DateTime.UtcNow;
    }

    public void AtualizarDados(
        string estado, string cidade, string bairro, string cep, string rua, int numero, string? complemento = null)
    {
        ValidarObrigatorio(estado, nameof(estado));
        ValidarObrigatorio(cidade, nameof(cidade));
        ValidarObrigatorio(bairro, nameof(bairro));
        ValidarCep(cep);
        ValidarObrigatorio(rua, nameof(rua));
        ValidarNumero(numero);

        Estado = estado;
        Cidade = cidade;
        Bairro = bairro;
        CEP = CepHelper.ApenasDigitos(cep);
        Rua = rua;
        Numero = numero;
        Complemento = complemento;
    }

    public void MarcarComoPadrao() => Padrao = true;

    public void DesmarcarComoPadrao() => Padrao = false;

    private void ValidarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }

    private void ValidarObrigatorio(string valor, string parametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentNullException(parametro, $"{parametro} é obrigatório!");
    }

    private void ValidarCep(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentNullException(nameof(cep), "CEP é obrigatório!");

        if (!CepHelper.FormatoValido(cep))
            throw new ArgumentException("CEP deve ter 8 dígitos.", nameof(cep));
    }

    private void ValidarNumero(int numero)
    {
        if (numero <= 0)
            throw new ArgumentException("Número deve ser maior que zero.", nameof(numero));
    }
}
