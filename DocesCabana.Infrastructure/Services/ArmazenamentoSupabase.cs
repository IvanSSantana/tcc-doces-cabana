using System.Net.Http.Headers;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using Microsoft.Extensions.Options;

namespace DocesCabana.Infrastructure.Services;

// Única implementação de IArmazenamentoDeImagem (spec 027 §10) — API REST
// simples do Supabase Storage, sem SDK novo, no mesmo desenho de
// FreteServiceMelhorEnvio (spec 020).
public class ArmazenamentoSupabase : IArmazenamentoDeImagem
{
    private const string MensagemDeFalhaPadrao = "Não foi possível enviar a imagem agora.";

    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;

    public ArmazenamentoSupabase(HttpClient httpClient, IOptions<SupabaseSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    // Nunca lança por falha de transporte (RN-03, Princípio VIII) —
    // indisponibilidade, credencial inválida e timeout são condição
    // esperada, e voltam como Mensagem.
    public async Task<ResultadoDoEnvioDeImagemDTO> Enviar(Stream conteudo, string nomeDoArquivoOriginal, string contentType)
    {
        // Chave vazia recusa sem tocar a rede (spec 027 §10, plano §4) —
        // determinístico e faz o E2E rodar offline, a lição que a spec 020
        // aprendeu tarde com o UserAgent em branco.
        if (string.IsNullOrWhiteSpace(_settings.ChaveDeServico))
            return ResultadoDoEnvioDeImagemDTO.ParaFalha("Armazenamento de imagem não configurado.");

        // RN-02/RF-07: quem nomeia o arquivo guardado é o adaptador, nunca o
        // nome que veio do computador de quem enviou.
        var extensao = Path.GetExtension(nomeDoArquivoOriginal);
        var nomeNoDestino = $"{Guid.NewGuid()}{extensao}";
        var caminho = $"{_settings.Bucket}/{_settings.Pasta}/{nomeNoDestino}";

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, $"/storage/v1/object/{caminho}")
        {
            Content = new StreamContent(conteudo)
        };
        mensagem.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        mensagem.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ChaveDeServico);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutEmSegundos));
            using var resposta = await _httpClient.SendAsync(mensagem, cts.Token);

            if (!resposta.IsSuccessStatusCode)
                return ResultadoDoEnvioDeImagemDTO.ParaFalha(MensagemDeFalhaPadrao);

            var urlPublica = $"{_settings.UrlBase}/storage/v1/object/public/{caminho}";
            return ResultadoDoEnvioDeImagemDTO.ParaSucesso(urlPublica);
        }
        catch (HttpRequestException)
        {
            return ResultadoDoEnvioDeImagemDTO.ParaFalha(MensagemDeFalhaPadrao);
        }
        catch (TaskCanceledException)
        {
            return ResultadoDoEnvioDeImagemDTO.ParaFalha(MensagemDeFalhaPadrao);
        }
    }
}
