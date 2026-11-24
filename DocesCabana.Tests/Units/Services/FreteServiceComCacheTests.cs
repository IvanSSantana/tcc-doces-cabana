using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class FreteServiceComCacheTests
{
    private readonly Mock<IFreteService> _internoMock = new();
    private readonly FreteServiceComCache _servico;

    private static readonly Guid ProdutoA = Guid.NewGuid();
    private static readonly Guid ProdutoB = Guid.NewGuid();

    public FreteServiceComCacheTests()
    {
        _servico = new FreteServiceComCache(_internoMock.Object, new MemoryCache(new MemoryCacheOptions()));
        _internoMock
            .Setup(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync((string cep, IReadOnlyList<LinhaDoCarrinhoDTO> _) =>
                new CotacaoDeFreteDTO(cep, [new OpcaoDeFreteDTO(1, "Correios", "PAC", 20m, 3, 5)], null));
    }

    private static LinhaDoCarrinhoDTO Linha(Guid produtoId, short quantidade, decimal preco = 10m) =>
        new() { ProdutoId = produtoId, Quantidade = quantidade, PrecoUnitario = preco };

    [Fact]
    public async Task Dado_MesmoCepEMesmosItens_Quando_CotarDeNovo_Entao_NaoDeveChamarOServicoInterno()
    {
        await _servico.Cotar("17340-001", [Linha(ProdutoA, 1), Linha(ProdutoB, 2)]);
        var segunda = await _servico.Cotar("17340001", [Linha(ProdutoB, 2), Linha(ProdutoA, 1)]);

        Assert.Single(segunda.Opcoes);
        _internoMock.Verify(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Once);
    }

    [Fact]
    public async Task Dado_QuantidadeDiferente_Quando_Cotar_Entao_DeveChamarOServicoInternoDeNovo()
    {
        await _servico.Cotar("17340001", [Linha(ProdutoA, 1)]);
        await _servico.Cotar("17340001", [Linha(ProdutoA, 2)]);

        _internoMock.Verify(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Dado_CepDiferente_Quando_Cotar_Entao_DeveChamarOServicoInternoDeNovo()
    {
        await _servico.Cotar("17340001", [Linha(ProdutoA, 1)]);
        await _servico.Cotar("01001000", [Linha(ProdutoA, 1)]);

        _internoMock.Verify(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Dado_CotacaoSemOpcoes_Quando_CotarDeNovo_Entao_NaoDeveGuardarAFalha()
    {
        _internoMock
            .Setup(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()))
            .ReturnsAsync(new CotacaoDeFreteDTO("17340001", [], "Não foi possível calcular o frete agora."));

        await _servico.Cotar("17340001", [Linha(ProdutoA, 1)]);
        await _servico.Cotar("17340001", [Linha(ProdutoA, 1)]);

        _internoMock.Verify(f => f.Cotar(It.IsAny<string>(), It.IsAny<IReadOnlyList<LinhaDoCarrinhoDTO>>()), Times.Exactly(2));
    }
}
