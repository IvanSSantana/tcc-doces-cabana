using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using Xunit;

namespace DocesCabana.Tests.Units.Mappings;

public class CarrinhoMapperTests
{
    private static Produto CriarProduto(decimal preco = 10m) =>
        new(Guid.NewGuid(), "Brigadeiro", preco, "https://imagem.com/brigadeiro.jpg");

    // RF-06/RN-02 (spec 021): sem cotação, o valor em destaque é o subtotal
    // e não inclui entrega nenhuma — dizer "total a pagar" aqui seria
    // afirmar um preço que ainda não existe.
    [Fact]
    public void Dado_NenhumaCotacao_Quando_Montar_Entao_ValorTotalDeveSerOSubtotalETemEntregaCalculadaDeveSerFalso()
    {
        var produto = CriarProduto(10m);

        var carrinho = CarrinhoMapper.Montar([(produto, (short)2)]);

        Assert.Equal(20m, carrinho.Subtotal);
        Assert.Equal(20m, carrinho.ValorTotal);
        Assert.False(carrinho.TemEntregaCalculada);
    }

    // RF-07/RN-06 (spec 021): havendo cotação com opções, o valor em
    // destaque passa a incluir a entrega — e é a mais barata que compõe o
    // total, por ser estimativa até o fechamento escolher de fato.
    [Fact]
    public void Dado_CotacaoComOpcoes_Quando_Montar_Entao_ValorTotalDeveIncluirAMaisBarataETemEntregaCalculadaDeveSerVerdadeiro()
    {
        var produto = CriarProduto(10m);
        var cotacao = new CotacaoDeFreteDTO(
            "01310000",
            [
                new OpcaoDeFreteDTO(2, "Correios", "SEDEX", 30m, 3, 4),
                new OpcaoDeFreteDTO(1, "Correios", "PAC", 18m, 8, 9),
            ],
            Mensagem: null);

        var carrinho = CarrinhoMapper.Montar([(produto, (short)1)], cotacao);

        Assert.True(carrinho.TemEntregaCalculada);
        Assert.Equal(28m, carrinho.ValorTotal); // 10 (subtotal) + 18 (PAC, a mais barata)
    }

    // CA-11 (spec 020, herdada): cotação sem opções (serviço fora do ar) não
    // é "entrega calculada" — o destaque continua sendo o subtotal.
    [Fact]
    public void Dado_CotacaoSemOpcoes_Quando_Montar_Entao_TemEntregaCalculadaDeveSerFalso()
    {
        var produto = CriarProduto(10m);
        var cotacaoFalha = new CotacaoDeFreteDTO("01310000", [], "Não foi possível calcular o frete agora.");

        var carrinho = CarrinhoMapper.Montar([(produto, (short)1)], cotacaoFalha);

        Assert.False(carrinho.TemEntregaCalculada);
        Assert.Equal(10m, carrinho.ValorTotal);
    }
}
