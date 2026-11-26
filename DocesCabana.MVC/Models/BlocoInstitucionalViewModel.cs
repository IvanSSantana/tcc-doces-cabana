namespace DocesCabana.MVC.Models;

/// <param name="ImagemRotulo">Texto alternativo da foto (ou do espaço reservado, quando não há foto).</param>
/// <param name="Imagem">Nome do arquivo em <c>wwwroot/images</c>. Nulo mantém o espaço reservado.</param>
public record BlocoInstitucionalViewModel(
    string Titulo,
    string Texto,
    string ImagemRotulo,
    bool Invertido,
    string? Imagem = null);
