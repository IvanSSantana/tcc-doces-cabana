namespace DocesCabana.Application.Enums;

// RN-06 (spec 017): "disponível para compra" é um estado só, com dois
// motivos de recusa — o que muda entre eles é a mensagem, não o efeito.
public enum MotivoIndisponibilidade
{
    Nenhum,
    ForaDoCatalogo,
    ForaDeEstoque
}
