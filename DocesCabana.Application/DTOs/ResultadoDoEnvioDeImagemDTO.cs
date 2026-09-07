namespace DocesCabana.Application.DTOs;

public record ResultadoDoEnvioDeImagemDTO(bool Sucesso, string? Url, string? Mensagem)
{
    public static ResultadoDoEnvioDeImagemDTO ParaSucesso(string url) => new(true, url, null);

    public static ResultadoDoEnvioDeImagemDTO ParaFalha(string mensagem) => new(false, null, mensagem);
}
