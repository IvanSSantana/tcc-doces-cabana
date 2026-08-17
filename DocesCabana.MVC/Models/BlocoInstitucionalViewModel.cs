namespace DocesCabana.MVC.Models;

// Modelo de apresentação de um bloco do Quem Somos (spec 009, RF-13/RF-14).
// Invertido troca o lado da imagem e do texto na grade, produzindo o
// ziguezague em torno do eixo — a ordem no DOM (título -> texto -> imagem)
// nunca muda, só a posição visual.
public record BlocoInstitucionalViewModel(
    string Titulo,
    string Texto,
    string ImagemRotulo,
    bool Invertido);
