using System.Text.Json.Serialization;

namespace DocesCabana.Infrastructure.Services.MelhorEnvio;

internal class RequisicaoDeCotacaoMelhorEnvio
{
    [JsonPropertyName("from")]
    public EnderecoMelhorEnvio From { get; set; } = new();

    [JsonPropertyName("to")]
    public EnderecoMelhorEnvio To { get; set; } = new();

    [JsonPropertyName("products")]
    public List<ProdutoMelhorEnvio> Products { get; set; } = [];

    [JsonPropertyName("options")]
    public OpcoesMelhorEnvio Options { get; set; } = new();
}

internal class EnderecoMelhorEnvio
{
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; } = string.Empty;
}

internal class ProdutoMelhorEnvio
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public decimal Width { get; set; }

    [JsonPropertyName("height")]
    public decimal Height { get; set; }

    [JsonPropertyName("length")]
    public decimal Length { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("insurance_value")]
    public decimal InsuranceValue { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

internal class OpcoesMelhorEnvio
{
    [JsonPropertyName("receipt")]
    public bool Receipt { get; set; }

    [JsonPropertyName("own_hand")]
    public bool OwnHand { get; set; }
}
