using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class EnderecoIntegrationTests : InfraestruturaSqliteEmMemoria
{
    [Fact]
    public async Task Dado_EnderecosCadastradosForaDeOrdem_Quando_BuscarPorUsuario_Entao_DeveOrdenarPorDataCadastro()
    {
        var usuarioId = await SemearUsuario();
        var maisRecente = new Endereco(usuarioId, "SP", "Barra Bonita", "Centro", "17340-000", "Rua Um", 1);
        var maisAntigo = new Endereco(usuarioId, "SP", "Bauru", "Jardim Europa", "17010-000", "Rua Dois", 2);
        // O construtor marca DataCadastro = DateTime.UtcNow — para provar a
        // ordenação sem depender de um Task.Delay real, ajusta direto via
        // reflection (mesmo recurso usado nos testes de serviço da 017).
        typeof(Endereco).GetProperty(nameof(Endereco.DataCadastro))!.SetValue(maisRecente, DateTime.UtcNow);
        typeof(Endereco).GetProperty(nameof(Endereco.DataCadastro))!.SetValue(maisAntigo, DateTime.UtcNow.AddDays(-5));
        Contexto.Enderecos.AddRange(maisRecente, maisAntigo);
        await Contexto.SaveChangesAsync();

        var repositorio = new EnderecoRepository(Contexto);
        var lista = await repositorio.BuscarPorUsuario(usuarioId);

        Assert.Equal(2, lista.Count);
        Assert.Equal(maisAntigo.EnderecoId, lista[0].EnderecoId);
        Assert.Equal(maisRecente.EnderecoId, lista[1].EnderecoId);
    }

    [Fact]
    public async Task Dado_EnderecosDeDuasPessoas_Quando_BuscarPorUsuario_Entao_NaoDeveTrazerODaOutra()
    {
        var usuarioUmId = await SemearUsuario("Cliente Um", "52998224725");
        var usuarioDoisId = await SemearUsuario("Cliente Dois", "11144477735");
        var enderecoDoUm = new Endereco(usuarioUmId, "SP", "Barra Bonita", "Centro", "17340-000", "Rua Um", 1);
        var enderecoDoDois = new Endereco(usuarioDoisId, "SP", "Bauru", "Jardim Europa", "17010-000", "Rua Dois", 2);
        Contexto.Enderecos.AddRange(enderecoDoUm, enderecoDoDois);
        await Contexto.SaveChangesAsync();

        var repositorio = new EnderecoRepository(Contexto);
        var listaDoUm = await repositorio.BuscarPorUsuario(usuarioUmId);

        var unico = Assert.Single(listaDoUm);
        Assert.Equal(enderecoDoUm.EnderecoId, unico.EnderecoId);
    }

    [Fact]
    public async Task Dado_EnderecoDeOutraPessoa_Quando_BuscarPeloPar_Entao_NaoDeveEncontrar()
    {
        // RN-05: o repositório busca sempre pelo par (enderecoId, usuarioId)
        // — é o desenho que sustenta o isolamento entre pessoas.
        var donoId = await SemearUsuario("Dono", "52998224725");
        var outraPessoaId = await SemearUsuario("Outra pessoa", "11144477735");
        var endereco = new Endereco(donoId, "SP", "Barra Bonita", "Centro", "17340-000", "Rua Um", 1);
        Contexto.Enderecos.Add(endereco);
        await Contexto.SaveChangesAsync();

        var repositorio = new EnderecoRepository(Contexto);
        var resultado = await repositorio.Buscar(endereco.EnderecoId, outraPessoaId);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Dado_EnderecoDoProprioUsuario_Quando_BuscarPeloPar_Entao_DeveEncontrar()
    {
        var usuarioId = await SemearUsuario();
        var endereco = new Endereco(usuarioId, "SP", "Barra Bonita", "Centro", "17340-000", "Rua Um", 1);
        Contexto.Enderecos.Add(endereco);
        await Contexto.SaveChangesAsync();

        var repositorio = new EnderecoRepository(Contexto);
        var resultado = await repositorio.Buscar(endereco.EnderecoId, usuarioId);

        Assert.NotNull(resultado);
        Assert.Equal(endereco.EnderecoId, resultado!.EnderecoId);
    }

    [Fact]
    public async Task Dado_EnderecoAdicionadoERemovido_Quando_SalvarEBuscarDeNovo_Entao_NaoDeveMaisExistir()
    {
        var usuarioId = await SemearUsuario();
        var endereco = new Endereco(usuarioId, "SP", "Barra Bonita", "Centro", "17340-000", "Rua Um", 1);
        var repositorio = new EnderecoRepository(Contexto);

        await repositorio.Adicionar(endereco);
        await Contexto.SaveChangesAsync();

        repositorio.Remover(endereco);
        await Contexto.SaveChangesAsync();

        var lista = await repositorio.BuscarPorUsuario(usuarioId);
        Assert.Empty(lista);
    }
}
