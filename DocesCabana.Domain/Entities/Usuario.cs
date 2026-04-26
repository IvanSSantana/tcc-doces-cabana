using System.Text.RegularExpressions;

namespace DocesCabana.Domain.Entities;

public class Usuario
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Senha { get; private set; }
    public string Celular { get; private set; }
    public DateTime DataNascimento { get; set; }
    public string CPF { get; private set; }

    public Usuario(string nome, string email, string senha, string celular, DateTime dataNascimento, string cpf, Guid id = default)
    {
        if (id == default)
            id = Guid.NewGuid();

        UsuarioId = id;
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarSenha(senha);
        ValidarCelular(celular);
        ValidarCPF(cpf);

        Nome = nome;
        Email = email;
        Senha = senha;
        Celular = celular;
        DataNascimento = dataNascimento;
        CPF = cpf;
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException("Nome é obrigatório!");
    }

    private void ValidarEmail(string email)
    {  
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException("Email é obrigatório!");

        // Regex oficial do HTML5 que segue RFC 5322
        Regex validacao_regex = new(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*$");

        if (!validacao_regex.IsMatch(email))
            throw new ArgumentException("Email inválido!");
    }

    private void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentNullException("Senha é obrigatória!");

        // Aceita CPFs já formatadas e somente números
        Regex validacao_regex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$");

        if (!validacao_regex.IsMatch(senha))
            throw new ArgumentException(
                "Senha deve ter no mínimo 6 caracteres, incluindo letra maiúscula, minúscula, número e caractere especial."
            );
    }

    private void ValidarCelular(string celular)
    {
        if (string.IsNullOrWhiteSpace(celular))
            throw new ArgumentNullException("Celular é obrigatório!");

        // Aceita números com ou sem formatação
        Regex validacao_regex = new(@"^(?:[14689][1-9]|2[12478]|3[1-5]|3[7-8]|5[1345]|7[134579])9\d{8}$");
        
        celular = new string(celular.Where(char.IsDigit).ToArray());

        if (!validacao_regex.IsMatch(celular))
            throw new ArgumentException("Número de celular inválido!");
    }

    private void ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentNullException("CPF é obrigatório!");

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            throw new ArgumentException("CPF inválido.");

        if (!CpfValido(cpf))
            throw new ArgumentException("CPF inválido.");
    }

    private bool CpfValido(string cpf)
    {
        int[] multiplicador1 = [10,9,8,7,6,5,4,3,2];
        int[] multiplicador2 = [11,10,9,8,7,6,5,4,3,2];

        var primeirosDigitosCPF = cpf.Substring(0, 9);
        var soma = primeirosDigitosCPF.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;

        primeirosDigitosCPF += digito;
        soma = primeirosDigitosCPF.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();

        resto = soma % 11;
        digito = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith(digito.ToString());
    }
}