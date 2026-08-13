using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public class Usuario
{
    public Guid UsuarioId { get; private set; }

    public string Nome { get; private set; } = default!;

    public string CPF { get; private set; } = default!;

    public string Celular { get; private set; } = default!;

    public DateTime DataNascimento { get; private set; }

    protected Usuario() { }

    // UsuarioId vem de fora (o Id da ContaDeAcesso já criada): a conta é a
    // principal na relação 1:1, o usuário é o dependente.
    public Usuario(Guid usuarioId, string nome, string cpf, string celular, DateTime dataNascimento)
    {
        ValidarUsuarioId(usuarioId);
        ValidarNome(nome);
        ValidarCPF(cpf);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);

        UsuarioId = usuarioId;
        Nome = nome;
        CPF = CpfHelper.ApenasDigitos(cpf);
        Celular = TelefoneHelper.ApenasDigitos(celular);
        DataNascimento = dataNascimento;
    }

    public void AtualizarDados(string nome, string celular, DateTime dataNascimento)
    {
        ValidarNome(nome);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);

        Nome = nome;
        Celular = TelefoneHelper.ApenasDigitos(celular);
        DataNascimento = dataNascimento;
    }

    private void ValidarUsuarioId(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");
    }

    private void ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentNullException(nameof(cpf), "CPF é obrigatório!");

        if (!CpfHelper.CpfValido(cpf))
            throw new ArgumentException("CPF inválido.", nameof(cpf));
    }

    private void ValidarCelular(string celular)
    {
        if (string.IsNullOrWhiteSpace(celular))
            throw new ArgumentNullException(nameof(celular), "Celular é obrigatório!");

        if (!TelefoneHelper.CelularValido(celular))
            throw new ArgumentException("Número de celular inválido.", nameof(celular));
    }

    private void ValidarDataNascimento(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;

        if (dataNascimento > hoje)
            throw new ArgumentException("Data de nascimento inválida.", nameof(dataNascimento));
        if (dataNascimento < hoje.AddYears(-120))
            throw new ArgumentException("Data de nascimento inválida.", nameof(dataNascimento));
    }
}
