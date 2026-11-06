using DocesCabana.Tests.Integration;

namespace DocesCabana.Tests.Integration.Infraestrutura;

public class MensagemDePostgresIndisponivelTests
{
    [Fact]
    public void Dado_QualquerCausa_Quando_Montar_Entao_DeveDizerParaLigarODockerDesktop()
    {
        var mensagem = MensagemDePostgresIndisponivel.Montar(new InvalidOperationException("causa qualquer"));

        Assert.Contains("Docker Desktop", mensagem);
    }

    [Fact]
    public void Dado_QualquerCausa_Quando_Montar_Entao_DeveDizerQueNaoUsaBancoDaMaquinaNemDoSupabase()
    {
        var mensagem = MensagemDePostgresIndisponivel.Montar(new InvalidOperationException("causa qualquer"));

        Assert.Contains("não usa banco nenhum da sua máquina nem do Supabase", mensagem);
    }

    [Fact]
    public void Dado_UmaCausaOriginal_Quando_Montar_Entao_DevePreservarAMensagemOriginalParaDiagnostico()
    {
        var causa = new InvalidOperationException("npipe: falha ao conectar ao daemon");

        var mensagem = MensagemDePostgresIndisponivel.Montar(causa);

        Assert.Contains("npipe: falha ao conectar ao daemon", mensagem);
    }
}
