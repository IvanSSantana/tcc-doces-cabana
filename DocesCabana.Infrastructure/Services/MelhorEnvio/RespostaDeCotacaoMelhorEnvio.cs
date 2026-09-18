using System.Text.Json.Serialization;

namespace DocesCabana.Infrastructure.Services.MelhorEnvio;

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

internal class RespostaDeErroMelhorEnvio
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
