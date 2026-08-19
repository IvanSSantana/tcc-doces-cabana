namespace DocesCabana.Application.Enums;

// Ordenação é escolha de consulta, não invariante do domínio (spec 012).
public enum OrdenacaoCatalogo
{
    // Anunciada, não oferecida (RF-16/RN-07) — indisponível até a spec 016
    // dar sentido a "venda" no sistema.
    MaisVendidos,
    MelhorAvaliados,
    MenorPreco,
    MaiorPreco,

    // Padrão (RF-17): é a única que não empata, o que a paginação exige
    // (RN-05) — nome é único por produto na prática, e mesmo empatando,
    // o repositório sempre desempata por Nome como critério final.
    NomeAZ,
}
