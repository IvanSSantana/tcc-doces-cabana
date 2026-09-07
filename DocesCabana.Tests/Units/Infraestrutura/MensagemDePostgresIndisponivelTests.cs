using DocesCabana.Tests.Integration;

namespace DocesCabana.Tests.Units.Infraestrutura;

// RF-09/RN-05, CA-07 (spec 028): quando falta o pré-requisito de ambiente
// para a suíte (Docker Desktop desligado), a mensagem precisa dizer o que
// ligar e o que a suíte não faz — não deixar o erro cru do Testcontainers
// (algo como "npipe" ou "daemon") falar sozinho. Mesma lição da 020
// (UserAgent vazio) e da 027 (UrlBase vazia): erro de ambiente descreve a
// causa, não o sintoma.
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
