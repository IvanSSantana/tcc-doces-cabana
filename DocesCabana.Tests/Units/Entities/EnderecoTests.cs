using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class EnderecoTests
{
    private readonly Guid _usuarioValido = Guid.NewGuid();
    private const string _cepValido = "17340-000";
    private const string _estadoValido = "São Paulo";
    private const string _cidadeValida = "Barra Bonita";
    private const string _bairroValido = "Centro";
    private const string _ruaValida = "Rua das Flores";
    private const int _numeroValido = 123;

    private Endereco CriarEndereco() =>
        new(_usuarioValido, _estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, _numeroValido);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarEndereco_Entao_DeveRetornarEnderecoInstanciado()
    {
        var endereco = CriarEndereco();

        Assert.NotNull(endereco);
        Assert.Equal("17340000", endereco.CEP);
        Assert.Null(endereco.Complemento);
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarEndereco_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Endereco(Guid.Empty, _estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, _numeroValido));
    }

    [Theory]
    [InlineData("1734000")]
    [InlineData("173400000")]
    public void Dado_CepComQuantidadeDeDigitosInvalida_Quando_CriarEndereco_Entao_DeveLancarArgumentException(string cep)
    {
        Assert.Throws<ArgumentException>(() =>
            new Endereco(_usuarioValido, _estadoValido, _cidadeValida, _bairroValido, cep, _ruaValida, _numeroValido));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Dado_EstadoInvalido_Quando_CriarEndereco_Entao_DeveLancarArgumentNullException(string? estado)
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Endereco(_usuarioValido, estado!, _cidadeValida, _bairroValido, _cepValido, _ruaValida, _numeroValido));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_NumeroInvalido_Quando_CriarEndereco_Entao_DeveLancarArgumentException(int numero)
    {
        Assert.Throws<ArgumentException>(() =>
            new Endereco(_usuarioValido, _estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, numero));
    }

    [Fact]
    public void Dado_ComplementoNulo_Quando_CriarEndereco_Entao_DeveAceitar()
    {
        var endereco = new Endereco(
            _usuarioValido, _estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, _numeroValido, complemento: null);

        Assert.Null(endereco.Complemento);
    }

    // ── Padrao/DataCadastro (spec 018) ──────────────────────────────────

    [Fact]
    public void Dado_EnderecoNovo_Quando_Criar_Entao_NaoDeveNascerPadraoEDeveMarcarDataCadastro()
    {
        var antes = DateTime.UtcNow;
        var endereco = CriarEndereco();
        var depois = DateTime.UtcNow;

        Assert.False(endereco.Padrao);
        Assert.InRange(endereco.DataCadastro, antes.AddSeconds(-1), depois.AddSeconds(1));
    }

    [Fact]
    public void Dado_Endereco_Quando_MarcarComoPadrao_Entao_PadraoDeveFicarVerdadeiro()
    {
        var endereco = CriarEndereco();

        endereco.MarcarComoPadrao();

        Assert.True(endereco.Padrao);
    }

    [Fact]
    public void Dado_EnderecoPadrao_Quando_DesmarcarComoPadrao_Entao_PadraoDeveFicarFalso()
    {
        var endereco = CriarEndereco();
        endereco.MarcarComoPadrao();

        endereco.DesmarcarComoPadrao();

        Assert.False(endereco.Padrao);
    }

    // ── AtualizarDados: mesmas validações do construtor ─────────────────

    [Fact]
    public void Dado_DadosValidos_Quando_AtualizarDados_Entao_DeveSubstituirTudo()
    {
        var endereco = CriarEndereco();

        endereco.AtualizarDados("Rio de Janeiro", "Angra dos Reis", "Bairro Novo", "23900-000", "Rua Nova", 456, "Casa 2");

        Assert.Equal("Rio de Janeiro", endereco.Estado);
        Assert.Equal("Angra dos Reis", endereco.Cidade);
        Assert.Equal("Bairro Novo", endereco.Bairro);
        Assert.Equal("23900000", endereco.CEP);
        Assert.Equal("Rua Nova", endereco.Rua);
        Assert.Equal(456, endereco.Numero);
        Assert.Equal("Casa 2", endereco.Complemento);
    }

    [Theory]
    [InlineData("1734000")]
    [InlineData("173400000")]
    public void Dado_CepComQuantidadeDeDigitosInvalida_Quando_AtualizarDados_Entao_DeveLancarArgumentException(string cep)
    {
        var endereco = CriarEndereco();

        Assert.Throws<ArgumentException>(() =>
            endereco.AtualizarDados(_estadoValido, _cidadeValida, _bairroValido, cep, _ruaValida, _numeroValido));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_NumeroInvalido_Quando_AtualizarDados_Entao_DeveLancarArgumentException(int numero)
    {
        var endereco = CriarEndereco();

        Assert.Throws<ArgumentException>(() =>
            endereco.AtualizarDados(_estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, numero));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Dado_RuaInvalida_Quando_AtualizarDados_Entao_DeveLancarArgumentNullException(string? rua)
    {
        var endereco = CriarEndereco();

        Assert.Throws<ArgumentNullException>(() =>
            endereco.AtualizarDados(_estadoValido, _cidadeValida, _bairroValido, _cepValido, rua!, _numeroValido));
    }

    [Fact]
    public void Dado_AtualizacaoInvalida_Quando_AtualizarDados_Entao_NaoDeveAlterarNadaAntesDeValidar()
    {
        // A validação roda antes de qualquer atribuição — o mesmo desenho do
        // construtor: um endereço nunca fica parcialmente atualizado.
        var endereco = CriarEndereco();

        Assert.Throws<ArgumentException>(() =>
            endereco.AtualizarDados(_estadoValido, _cidadeValida, _bairroValido, _cepValido, _ruaValida, 0));

        Assert.Equal(_estadoValido, endereco.Estado);
        Assert.Equal(_numeroValido, endereco.Numero);
    }
}
