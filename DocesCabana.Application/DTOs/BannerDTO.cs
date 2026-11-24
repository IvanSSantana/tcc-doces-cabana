using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

/// <summary>
/// Formulário de cadastro e edição de banner. A imagem não chega por aqui: o controlador envia
/// o arquivo ao armazenamento e passa a URL ao serviço. <see cref="ImagemUrl"/> só exibe a atual.
/// Os três seletores de destino podem vir preenchidos (sem JavaScript todos aparecem);
/// vale só o do <see cref="TipoDeDestino"/>.
/// </summary>
public class BannerDTO
{
    public Guid BannerId { get; set; }

    public string TextoAlternativo { get; set; } = string.Empty;

    public short? Ordem { get; set; }

    public DateOnly? DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public TipoDeDestinoDoBanner TipoDeDestino { get; set; }

    public Guid? CategoriaId { get; set; }

    public Guid? SubcategoriaId { get; set; }

    public Guid? ProdutoId { get; set; }

    public string? ImagemUrl { get; set; }
}
