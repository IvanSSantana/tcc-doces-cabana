using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class EnderecoServiceTests
{
    private readonly Mock<IEnderecoRepository> _enderecoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly EnderecoService _enderecoService;

    public EnderecoServiceTests()
    {
        _enderecoRepositoryMock = new Mock<IEnderecoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _enderecoService = new EnderecoService(_enderecoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static EnderecoDTO CriarDTO(Guid enderecoId = default) => new()
    {
        EnderecoId = enderecoId,
        Estado = "São Paulo",
        Cidade = "Barra Bonita",
        Bairro = "Centro",
        CEP = "17340-000",
        Rua = "Rua das Flores",
        Numero = 123,
    };

    private static Endereco CriarEndereco(Guid usuarioId, DateTime dataCadastro, bool padrao = false)
    {
        var endereco = new Endereco(usuarioId, "São Paulo", "Barra Bonita", "Centro", "17340-000", "Rua das Flores", 123);
        typeof(Endereco).GetProperty(nameof(Endereco.DataCadastro))!.SetValue(endereco, dataCadastro);
        if (padrao) endereco.MarcarComoPadrao();
        return endereco;
    }

    // ── Cadastrar — RN-02 ───────────────────────────────────────────────

    [Fact]
    public async Task Dado_NenhumEnderecoExistente_Quando_Cadastrar_Entao_DeveNascerPrincipal()
    {
        var usuarioId = Guid.NewGuid();
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([]);

        await _enderecoService.Cadastrar(CriarDTO(), usuarioId);

        _enderecoRepositoryMock.Verify(r => r.Adicionar(
            It.Is<Endereco>(e => e.Padrao && e.UsuarioId == usuarioId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_UmEnderecoPrincipalJaExistente_Quando_CadastrarOSegundo_Entao_OPrimeiroDeveContinuarPrincipal()
    {
        var usuarioId = Guid.NewGuid();
        var existente = CriarEndereco(usuarioId, DateTime.UtcNow.AddDays(-1), padrao: true);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([existente]);

        await _enderecoService.Cadastrar(CriarDTO(), usuarioId);

        _enderecoRepositoryMock.Verify(r => r.Adicionar(
            It.Is<Endereco>(e => !e.Padrao)), Times.Once);
        Assert.True(existente.Padrao);
    }

    // ── TornarPrincipal — RN-03 ─────────────────────────────────────────

    [Fact]
    public async Task Dado_DoisEnderecos_Quando_TornarOSegundoPrincipal_Entao_OPrimeiroDeveDeixarDeSer()
    {
        var usuarioId = Guid.NewGuid();
        var primeiro = CriarEndereco(usuarioId, DateTime.UtcNow.AddDays(-1), padrao: true);
        var segundo = CriarEndereco(usuarioId, DateTime.UtcNow, padrao: false);
        _enderecoRepositoryMock.Setup(r => r.Buscar(segundo.EnderecoId, usuarioId)).ReturnsAsync(segundo);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([primeiro, segundo]);

        await _enderecoService.TornarPrincipal(segundo.EnderecoId, usuarioId);

        Assert.False(primeiro.Padrao);
        Assert.True(segundo.Padrao);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    // ── Excluir — RN-04 ─────────────────────────────────────────────────

    [Fact]
    public async Task Dado_DoisEnderecos_Quando_ExcluirOPrincipal_Entao_OMaisAntigoDosRestantesDevePromover()
    {
        var usuarioId = Guid.NewGuid();
        var principal = CriarEndereco(usuarioId, DateTime.UtcNow.AddDays(-2), padrao: true);
        var maisAntigoRestante = CriarEndereco(usuarioId, DateTime.UtcNow.AddDays(-1), padrao: false);
        var maisRecente = CriarEndereco(usuarioId, DateTime.UtcNow, padrao: false);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId))
            .ReturnsAsync([principal, maisAntigoRestante, maisRecente]);

        await _enderecoService.Excluir(principal.EnderecoId, usuarioId);

        _enderecoRepositoryMock.Verify(r => r.Remover(principal), Times.Once);
        Assert.True(maisAntigoRestante.Padrao);
        Assert.False(maisRecente.Padrao);
    }

    [Fact]
    public async Task Dado_DoisEnderecos_Quando_ExcluirOQueNaoEPrincipal_Entao_OPrincipalDeveContinuar()
    {
        var usuarioId = Guid.NewGuid();
        var principal = CriarEndereco(usuarioId, DateTime.UtcNow.AddDays(-1), padrao: true);
        var comum = CriarEndereco(usuarioId, DateTime.UtcNow, padrao: false);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([principal, comum]);

        await _enderecoService.Excluir(comum.EnderecoId, usuarioId);

        _enderecoRepositoryMock.Verify(r => r.Remover(comum), Times.Once);
        _enderecoRepositoryMock.Verify(r => r.Remover(principal), Times.Never);
        Assert.True(principal.Padrao);
    }

    [Fact]
    public async Task Dado_UnicoEndereco_Quando_Excluir_Entao_NaoDeveSobrarNenhumComPrincipal()
    {
        var usuarioId = Guid.NewGuid();
        var unico = CriarEndereco(usuarioId, DateTime.UtcNow, padrao: true);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([unico]);

        await _enderecoService.Excluir(unico.EnderecoId, usuarioId);

        _enderecoRepositoryMock.Verify(r => r.Remover(unico), Times.Once);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_EnderecoInexistenteParaOUsuario_Quando_Excluir_Entao_DeveLancarKeyNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _enderecoService.Excluir(Guid.NewGuid(), usuarioId));

        _enderecoRepositoryMock.Verify(r => r.Remover(It.IsAny<Endereco>()), Times.Never);
    }

    // ── Editar ────────────────────────────────────────────────────────

    [Fact]
    public async Task Dado_EnderecoDoUsuario_Quando_Editar_Entao_DeveAtualizarDados()
    {
        var usuarioId = Guid.NewGuid();
        var endereco = CriarEndereco(usuarioId, DateTime.UtcNow);
        _enderecoRepositoryMock.Setup(r => r.Buscar(endereco.EnderecoId, usuarioId)).ReturnsAsync(endereco);
        var dto = CriarDTO(endereco.EnderecoId);
        dto.Numero = 456;

        await _enderecoService.Editar(dto, usuarioId);

        Assert.Equal(456, endereco.Numero);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Once);
    }

    [Fact]
    public async Task Dado_EnderecoInexistenteParaOUsuario_Quando_Editar_Entao_DeveLancarKeyNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _enderecoRepositoryMock.Setup(r => r.Buscar(It.IsAny<Guid>(), usuarioId)).ReturnsAsync((Endereco?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _enderecoService.Editar(CriarDTO(Guid.NewGuid()), usuarioId));
    }

    // ── Listar / Buscar ───────────────────────────────────────────────

    [Fact]
    public async Task Dado_EnderecosCadastrados_Quando_ListarDoUsuario_Entao_DeveMapearTodos()
    {
        var usuarioId = Guid.NewGuid();
        var endereco = CriarEndereco(usuarioId, DateTime.UtcNow, padrao: true);
        _enderecoRepositoryMock.Setup(r => r.BuscarPorUsuario(usuarioId)).ReturnsAsync([endereco]);

        var lista = await _enderecoService.ListarDoUsuario(usuarioId);

        var dto = Assert.Single(lista);
        Assert.Equal(endereco.EnderecoId, dto.EnderecoId);
        Assert.True(dto.Padrao);
    }

    [Fact]
    public async Task Dado_EnderecoInexistenteParaOUsuario_Quando_BuscarDoUsuario_Entao_DeveLancarKeyNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _enderecoRepositoryMock.Setup(r => r.Buscar(It.IsAny<Guid>(), usuarioId)).ReturnsAsync((Endereco?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _enderecoService.BuscarDoUsuario(Guid.NewGuid(), usuarioId));
    }

    // ── Isolamento entre pessoas — RN-05 (Fase 5) ───────────────────────
    // Buscar, Editar e Excluir já são cobertos acima ("inexistente para o
    // usuário" e "endereço de outra pessoa" são, do ponto de vista do
    // serviço, exatamente a mesma coisa: o repositório busca sempre pelo
    // par (enderecoId, usuarioId) e devolve null nos dois casos — é esse
    // desenho (T015) que torna a RN-05 difícil de violar por esquecimento.
    // Falta só TornarPrincipal.

    [Fact]
    public async Task Dado_EnderecoDeOutraPessoa_Quando_TentarTornarPrincipal_Entao_DeveLancarKeyNotFoundExceptionSemAlterarNada()
    {
        var usuarioId = Guid.NewGuid();
        var enderecoDeOutraPessoa = CriarEndereco(Guid.NewGuid(), DateTime.UtcNow, padrao: true);
        _enderecoRepositoryMock.Setup(r => r.Buscar(enderecoDeOutraPessoa.EnderecoId, usuarioId)).ReturnsAsync((Endereco?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _enderecoService.TornarPrincipal(enderecoDeOutraPessoa.EnderecoId, usuarioId));

        // Nada foi alterado: o endereço alheio continua principal, e nenhum
        // BuscarPorUsuario/SalvarAlteracoes chegou a rodar.
        Assert.True(enderecoDeOutraPessoa.Padrao);
        _enderecoRepositoryMock.Verify(r => r.BuscarPorUsuario(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(default), Times.Never);
    }
}
