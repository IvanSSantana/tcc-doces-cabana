using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Services.MelhorEnvio;
using Microsoft.Extensions.Options;

namespace DocesCabana.Infrastructure.Services;

// Única implementação de IFreteService (spec 020 §10) — sem simulador ao
// lado: o MelhorEnvio já calcula peso cubado e empacota sozinho, e uma
// versão nossa criaria uma segunda resposta para a mesma pergunta, sem como
// conferir uma contra a outra.
public class FreteServiceMelhorEnvio : IFreteService
{
    private readonly HttpClient _httpClient;
    private readonly FreteSettings _settings;

    public FreteServiceMelhorEnvio(HttpClient httpClient, IOptions<FreteSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    // Nunca lança por falha de transporte (RN-02, Princípio VIII) —
    // indisponibilidade, CEP não atendido, credencial inválida e timeout
    // são condição esperada, e voltam como Mensagem.
    public async Task<CotacaoDeFreteDTO> Cotar(string cepDestino, IReadOnlyList<LinhaDoCarrinhoDTO> itensDisponiveis)
    {
        var requisicao = MontarRequisicao(cepDestino, itensDisponiveis);

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/v2/me/shipment/calculate")
        {
            Content = JsonContent.Create(requisicao)
        };
        mensagem.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        mensagem.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);
        // Obrigatório pela API (spec 020 §10) — sem ele a API recusa.
        mensagem.Headers.UserAgent.ParseAdd(_settings.UserAgent);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutEmSegundos));
            using var resposta = await _httpClient.SendAsync(mensagem, cts.Token);

            if (!resposta.IsSuccessStatusCode)
                return CotacaoIndisponivel(cepDestino, await LerMensagemDeErro(resposta));

            var corpo = await resposta.Content.ReadFromJsonAsync<List<RespostaDeCotacaoMelhorEnvio>>(cancellationToken: cts.Token);
            if (corpo is null)
                return CotacaoIndisponivel(cepDestino, "Não foi possível calcular o frete agora.");

            var opcoes = MapearOpcoes(corpo);
            return new CotacaoDeFreteDTO(cepDestino, opcoes, opcoes.Count > 0 ? null : "Não foi possível calcular o frete agora.");
        }
        catch (HttpRequestException)
        {
            return CotacaoIndisponivel(cepDestino, "Não foi possível calcular o frete agora.");
        }
        catch (TaskCanceledException)
        {
            return CotacaoIndisponivel(cepDestino, "Não foi possível calcular o frete agora.");
        }
        catch (System.Text.Json.JsonException)
        {
            return CotacaoIndisponivel(cepDestino, "Não foi possível calcular o frete agora.");
        }
    }

    private RequisicaoDeCotacaoMelhorEnvio MontarRequisicao(string cepDestino, IReadOnlyList<LinhaDoCarrinhoDTO> itens) => new()
    {
        From = new EnderecoMelhorEnvio { PostalCode = _settings.CepDeOrigem },
        To = new EnderecoMelhorEnvio { PostalCode = ApenasDigitos(cepDestino) },
        Products = itens.Select(item => new ProdutoMelhorEnvio
        {
            Id = item.ProdutoId.ToString(),
            Width = item.Largura,
            Height = item.Altura,
            Length = item.Comprimento,
            Weight = item.Peso,
            InsuranceValue = item.PrecoUnitario,
            Quantity = item.Quantidade
        }).ToList(),
        Options = new OpcoesMelhorEnvio { Receipt = false, OwnHand = false }
    };

    private static string ApenasDigitos(string valor) =>
        new string(valor.Where(char.IsDigit).ToArray());

    // Armadilha 3 (plano §4): entrada sem preço utilizável é descartada —
    // a documentação obtida só mostrou o caso de sucesso, então isso é
    // defensivo, não confirmado.
    private static List<OpcaoDeFreteDTO> MapearOpcoes(IEnumerable<RespostaDeCotacaoMelhorEnvio> respostas) =>
        respostas
            .Where(r => r.Error is null && TryParsePreco(r.CustomPrice, out _) && r.CustomDeliveryRange is not null)
            .Select(r =>
            {
                TryParsePreco(r.CustomPrice, out var preco);
                return new OpcaoDeFreteDTO(
                    r.Id,
                    r.Company?.Name ?? "Transportadora",
                    r.Name ?? "Serviço",
                    preco,
                    r.CustomDeliveryRange!.Min,
                    r.CustomDeliveryRange.Max);
            })
            .ToList();

    // Armadilha 2 (plano §4): o preço vem como string com ponto decimal, e
    // a aplicação é pt-BR — Parse sem InvariantCulture transformaria
    // "37.79" em 3779, não 37,79.
    private static bool TryParsePreco(string? valor, out decimal preco) =>
        decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out preco);

    private static async Task<string> LerMensagemDeErro(HttpResponseMessage resposta)
    {
        try
        {
            var erro = await resposta.Content.ReadFromJsonAsync<RespostaDeErroMelhorEnvio>();
            return erro?.Message ?? "Não foi possível calcular o frete agora.";
        }
        catch
        {
            return "Não foi possível calcular o frete agora.";
        }
    }

    private static CotacaoDeFreteDTO CotacaoIndisponivel(string cepDestino, string mensagem) =>
        new(cepDestino, [], mensagem);
}
