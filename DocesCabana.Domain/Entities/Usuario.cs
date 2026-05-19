using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Domain.Entities;

public class Usuario : IdentityUser<Guid>
{
    public string Nome { get; private set; } = default!;
    public DateTime DataNascimento { get; set; }
    public string CPF { get; private set; } = default!;

    protected Usuario() { }

    public Usuario(string nome, string email, string celular, DateTime dataNascimento, string cpf, Guid id = default)
    {
        if (id == default)
            id = Guid.NewGuid();

        Id = id;
        UserName = email;
        Email = email;
        PhoneNumber = celular;

        ValidarNome(nome);
        ValidarEmail(email);
        ValidarCelular(celular);
        ValidarDataNascimento(dataNascimento);
        ValidarCPF(cpf);

        Nome = nome;
        DataNascimento = dataNascimento;
        CPF = cpf;
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            // ArgumentException indica que o argumento é inválido, nameof(nome) informa qual parâmetro causou o erro
            throw new ArgumentException("Nome é obrigatório!", nameof(nome));
    }

    private void ValidarEmail(string email)
    {  
        if (string.IsNullOrWhiteSpace(email))
            // ArgumentException indica que o argumento é inválido, nameof(email) informa qual parâmetro causou o erro
            throw new ArgumentException("Email é obrigatório!", nameof(email));

        // Regex oficial do HTML5 que segue RFC 5322
        Regex validacao_regex = new(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*$");

        if (!validacao_regex.IsMatch(email))
            // ArgumentException indica que o argumento é inválido, nameof(email) informa qual parâmetro causou o erro
            throw new ArgumentException("Email inválido!", nameof(email));
    }

    // Mover para Identity
    private void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            // ArgumentException indica que o argumento é inválido, nameof(senha) informa qual parâmetro causou o erro
            throw new ArgumentException("Senha é obrigatória!", nameof(senha));

        Regex validacao_regex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$");

        if (!validacao_regex.IsMatch(senha))
            throw new ArgumentException(
                "Senha deve ter no mínimo 6 caracteres, incluindo letra maiúscula, minúscula, número e caractere especial.",
                nameof(senha)
            );
    }

    private void ValidarCelular(string celular)
    {
        if (string.IsNullOrWhiteSpace(celular))
            // ArgumentException indica que o argumento é inválido, nameof(celular) informa qual parâmetro causou o erro
            throw new ArgumentException("Celular é obrigatório!", nameof(celular));

        // Aceita números com ou sem formatação
        Regex validacao_regex = new(@"^(?:[14689][1-9]|2[12478]|3[1-5]|3[7-8]|5[1345]|7[134579])9\d{8}$");
        
        celular = new string(celular.Where(char.IsDigit).ToArray());

        if (!validacao_regex.IsMatch(celular))
            throw new ArgumentException("Número de celular inválido!", nameof(celular));
    }

    // Valida se a data de nascimento não é no futuro e nem absurda (mais de 120 anos atrás)
    private void ValidarDataNascimento(DateTime dataNascimento)
    {
        var hoje = DateTime.Today; // Usa Today ao invés de Now para ignorar a hora e comparar apenas a data

        if (dataNascimento > hoje)
            throw new ArgumentException("Data de nascimento não pode ser no futuro", nameof(dataNascimento));
        if (dataNascimento < hoje.AddYears(-120))
            throw new ArgumentException("Data de nascimento inválida", nameof(dataNascimento));
    }
    
    private void ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            // ArgumentException indica que o argumento é inválido, nameof(cpf) informa qual parâmetro causou o erro
            throw new ArgumentException("CPF é obrigatório!", nameof(cpf));

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            throw new ArgumentException("CPF inválido.", nameof(cpf));

        if (!CpfValido(cpf))
            throw new ArgumentException("CPF inválido.", nameof(cpf));
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

        // Verifica se o CPF termina com o dígito verificador calculado
        return cpf.EndsWith(digito.ToString());
    }
}