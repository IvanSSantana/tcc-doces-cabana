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
}
