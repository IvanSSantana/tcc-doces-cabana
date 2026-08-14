using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class AvaliacaoMapper
{
    public static AvaliacaoDTO ToDTO(Avaliacao avaliacao, Guid? usuarioAtual) =>
        new()
        {
            AvaliacaoId = avaliacao.AvaliacaoId,
            AutorNome = avaliacao.Usuario?.Nome ?? string.Empty,
            Nota = avaliacao.Nota,
            Comentario = avaliacao.Comentario,
            DataCriacao = avaliacao.DataCriacao,
            TotalUteis = avaliacao.TotalUteis,
            MarcadaPeloUsuarioAtual = usuarioAtual.HasValue && avaliacao.MarcadaComoUtilPor(usuarioAtual.Value),
            EhDoUsuarioAtual = usuarioAtual.HasValue && avaliacao.UsuarioId == usuarioAtual.Value
        };

    public static List<AvaliacaoDTO> ToDTO(IEnumerable<Avaliacao> avaliacoes, Guid? usuarioAtual) =>
        avaliacoes.Select(a => ToDTO(a, usuarioAtual)).ToList();
}
