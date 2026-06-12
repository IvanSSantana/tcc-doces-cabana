using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity;

public class Usuario : IdentityUser<Guid>
{
    public string Nome { get; private set; } = default!;
    public DateTime DataNascimento { get; private set; }
    public string CPF { get; private set; } = default!;

    protected Usuario() { }

    public Usuario(string nome, string email, string celular, DateTime dataNascimento, string cpf)
    {
        UserName = email;
        Email = email;
        PhoneNumber = celular;

        ValidarNome(nome);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);
        ValidarCPF(cpf);

        Nome = nome;
        DataNascimento = dataNascimento;
        CPF = cpf;
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
    
    private void ValidarCelular(string celular)
    {
        if (string.IsNullOrWhiteSpace(celular))
            throw new ArgumentNullException(nameof(celular), "Celular é obrigatório!");

        Regex validacaoRegex = new(@"^(?:[14689][1-9]|2[12478]|3[1-5]|3[7-8]|5[1345]|7[134579])9\d{8}$");
        
        celular = new string(celular.Where(char.IsDigit).ToArray());

        if (!validacaoRegex.IsMatch(celular))
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

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            throw new ArgumentException("CPF inválido.", nameof(cpf));

        if (!CpfValido(cpf))
            throw new ArgumentException("CPF inválido.", nameof(cpf));
    }

    private bool CpfValido(string cpf)
    {
        if (new string(cpf[0], 11) == cpf)
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var primeirasDigitosCPF = cpf.Substring(0, 9);
        var soma = primeirasDigitosCPF.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;


        primeirasDigitosCPF += digito;
        soma = primeirasDigitosCPF.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();

        resto = soma % 11;
        digito = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith(digito.ToString());
    }
}
