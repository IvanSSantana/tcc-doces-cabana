using System.Net.Http.Headers;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using Microsoft.Extensions.Options;

namespace DocesCabana.Infrastructure.Services;

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

    public async Task<ResultadoDoEnvioDeImagemDTO> Enviar(Stream conteudo, string nomeDoArquivoOriginal, string contentType)
    {
        if (string.IsNullOrWhiteSpace(_settings.ChaveDeServico))
            return ResultadoDoEnvioDeImagemDTO.ParaFalha("Armazenamento de imagem não configurado.");

        var extensao = Path.GetExtension(nomeDoArquivoOriginal);
        var nomeNoDestino = $"{Guid.NewGuid()}{extensao}";
        var caminho = $"{_settings.Bucket}/{_settings.Pasta}/{nomeNoDestino}";

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, $"/storage/v1/object/{caminho}")
        {
            Content = new StreamContent(conteudo)
        };
        mensagem.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        mensagem.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ChaveDeServico);

        mensagem.Headers.Add("apikey", _settings.ChaveDeServico);

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
