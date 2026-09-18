using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IArmazenamentoDeImagem
{
    Task<ResultadoDoEnvioDeImagemDTO> Enviar(Stream conteudo, string nomeDoArquivoOriginal, string contentType);
}
