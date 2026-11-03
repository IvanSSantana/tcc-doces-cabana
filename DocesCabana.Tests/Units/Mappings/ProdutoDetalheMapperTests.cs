using DocesCabana.Application.Mappings;

namespace DocesCabana.Tests.Units.Mappings;

public class ProdutoDetalheMapperTests
{
    [Fact]
    public void Dado_DescricaoNula_Quando_GerarResumo_Entao_DeveRetornarNulo()
    {
        var resumo = ProdutoDetalheMapper.GerarResumo(null);

        Assert.Null(resumo);
    }

    [Fact]
    public void Dado_DescricaoVazia_Quando_GerarResumo_Entao_DeveRetornarNulo()
    {
        var resumo = ProdutoDetalheMapper.GerarResumo("");

        Assert.Null(resumo);
    }

    [Fact]
    public void Dado_DescricaoComCentoESessentaCaracteresOuMenos_Quando_GerarResumo_Entao_DeveRetornarInteiraSemReticencias()
    {
        var descricao = new string('a', 160);

        var resumo = ProdutoDetalheMapper.GerarResumo(descricao);

        Assert.Equal(descricao, resumo);
        Assert.DoesNotContain('…', resumo!);
    }

    [Fact]
    public void Dado_DescricaoComMenosDeCentoESessentaCaracteres_Quando_GerarResumo_Entao_DeveRetornarInteiraSemReticencias()
    {
        var descricao = "Doce caseiro feito com leite e açúcar.";

        var resumo = ProdutoDetalheMapper.GerarResumo(descricao);

        Assert.Equal(descricao, resumo);
    }

    [Fact]
    public void Dado_DescricaoComMaisDeCentoESessentaCaracteres_Quando_GerarResumo_Entao_DeveCortarNoFimDaPalavraComReticencias()
    {
        var palavras = Enumerable.Range(1, 40).Select(i => $"palavra{i:D2}");
        var descricao = string.Join(" ", palavras);

        var resumo = ProdutoDetalheMapper.GerarResumo(descricao);

        Assert.NotNull(resumo);
        Assert.EndsWith("…", resumo);
        Assert.True(resumo!.Length <= 161, $"Resumo tem {resumo.Length} caracteres, esperado no máximo 161 (160 + reticências).");

        var semReticencias = resumo[..^1];
        Assert.DoesNotContain(" …", resumo);
        Assert.StartsWith(semReticencias, descricao);

        var proximoCaractere = descricao.Length > semReticencias.Length
            ? descricao[semReticencias.Length]
            : (char?)null;
        Assert.True(proximoCaractere is null or ' ');
    }
}
