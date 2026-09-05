using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IArmazenamentoDeImagem
{
    // Nunca lança por falha de transporte: credencial ausente, serviço fora
    // do ar ou arquivo recusado voltam no resultado (RN-03, Princípio VIII).
    // O nome que chega é o do computador de quem enviou e serve só para
    // derivar a extensão — quem nomeia o arquivo guardado é o adaptador
    // (RN-02).
    Task<ResultadoDoEnvioDeImagemDTO> Enviar(Stream conteudo, string nomeDoArquivoOriginal, string contentType);
}
