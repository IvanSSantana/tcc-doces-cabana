namespace DocesCabana.Application.DTOs;

// Só metadados — o conteúdo do arquivo não passa pelo validador (spec 027,
// plano §4).
public record ImagemParaEnvioDTO(string NomeDoArquivo, string ContentType, long TamanhoEmBytes);
