using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.MVC.Models;

/// <summary>
/// A avaliação mais o contexto que o formulário de voto precisa preservar ao
/// redirecionar de volta — mesma ordenação e mesma quantidade de avaliações
/// abertas (RF-17).
/// </summary>
public record AvaliacaoCartaoViewModel(AvaliacaoDTO Avaliacao, Guid ProdutoId, OrdenacaoAvaliacao Ordenacao, int Exibir);
