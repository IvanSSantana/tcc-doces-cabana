using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.MVC.Models;

public record AvaliacaoCartaoViewModel(AvaliacaoDTO Avaliacao, Guid ProdutoId, OrdenacaoAvaliacao Ordenacao, int Exibir);
