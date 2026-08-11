using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using DocesCabana.Domain.Helpers; 

namespace DocesCabana.Infrastructure.Identity;

public class Usuario : IdentityUser<Guid>
{
    public string Nome { get; private set; } = default!;
    public DateTime DataNascimento { get; private set; }
    public string CPF { get; private set; } = default!;

    protected Usuario() { }

    public Usuario(string nome, string email, string celular, DateTime dataNascimento, string cpf)
    {
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);
        ValidarCPF(cpf);

        UserName = email;
        PhoneNumber = celular;
        Nome = nome;
        Email = email;
        CPF = cpf;
        DataNascimento = dataNascimento;
    }

    public void AtualizarDados(string nome, string celular, DateTime dataNascimento)
    {
        ValidarNome(nome);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);

        Nome = nome;
        PhoneNumber = celular;
        DataNascimento = dataNascimento;
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");
    }

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled);

    private void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email é obrigatório!");

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Email inválido.", nameof(email));
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
    
    private void ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentNullException(nameof(cpf), "CPF é obrigatório!");

        if (!CpfHelper.CpfValido(cpf))
            throw new ArgumentException("CPF inválido.", nameof(cpf));
    }
}
