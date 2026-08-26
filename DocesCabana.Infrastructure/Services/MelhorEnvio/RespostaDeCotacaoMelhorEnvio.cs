using System.Text.Json.Serialization;

namespace DocesCabana.Infrastructure.Services.MelhorEnvio;

// Uma entrada do array que a API devolve. custom_price e
// custom_delivery_range são os campos a usar — não price/delivery_time,
// que são o valor "de tabela" sem as customizações da conta (plano §4,
// armadilha 1).
internal class RespostaDeCotacaoMelhorEnvio
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("custom_price")]
    public string? CustomPrice { get; set; }

    [JsonPropertyName("custom_delivery_range")]
    public FaixaDePrazoMelhorEnvio? CustomDeliveryRange { get; set; }

    [JsonPropertyName("company")]
    public EmpresaMelhorEnvio? Company { get; set; }

    // A documentação obtida não mostrou o formato de uma entrada sem
    // serviço disponível — este campo é defensivo, não confirmado: se
    // existir e vier preenchido, ou se custom_price não vier utilizável, a
    // entrada é descartada (RF-06: toda opção exibida tem preço e prazo).
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

internal class FaixaDePrazoMelhorEnvio
{
    [JsonPropertyName("min")]
    public int Min { get; set; }

    [JsonPropertyName("max")]
    public int Max { get; set; }
}

internal class EmpresaMelhorEnvio
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

// Resposta de erro (422) — spec 020, exemplo da documentação.
internal class RespostaDeErroMelhorEnvio
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
