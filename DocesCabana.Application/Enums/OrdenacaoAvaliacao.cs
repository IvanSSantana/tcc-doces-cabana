namespace DocesCabana.Application.Enums;

// Ordenação é escolha de consulta, não invariante do domínio — por isso mora
// aqui, não em Domain/Enums (RF-16 da spec 008).
public enum OrdenacaoAvaliacao
{
    Relevantes,
    MaisRecentes,
    MaiorNota,
    MenorNota
}
