using DocesCabana.Domain.Enums;

namespace DocesCabana.Domain.Entities;

public class Promocao
{
    public Guid PromocaoId { get; private set; }

    public string Nome { get; private set; } = default!;

    public string? Descricao { get; private set; }

    public PromocaoTipo Tipo { get; private set; }

    public decimal Valor { get; private set; }

    public DateTime DataInicio { get; private set; }

    public DateTime DataFim { get; private set; }

    public bool Ativa { get; private set; }

    protected Promocao() { }

    public Promocao(
        string nome,
        PromocaoTipo tipo,
        decimal valor,
        DateTime dataInicio,
        DateTime dataFim,
        string? descricao = null,
        Guid id = default)
    {
        ValidarNome(nome);
        ValidarPeriodo(dataInicio, dataFim);
        ValidarValor(tipo, valor);

        PromocaoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        Nome = nome;
        Descricao = descricao;
        Tipo = tipo;
        Valor = valor;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Ativa = true;
    }

    public void Ativar() => Ativa = true;

    public void Desativar() => Ativa = false;

    public void AlterarPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        ValidarPeriodo(dataInicio, dataFim);

        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public bool EstaVigente(DateTime referencia) =>
        Ativa && referencia >= DataInicio && referencia <= DataFim;

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        if (nome.Length > 255)
            throw new ArgumentException("Nome deve ter no máximo 255 caracteres.", nameof(nome));
    }

    private void ValidarPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim <= dataInicio)
            throw new ArgumentException("Data de fim deve ser posterior à data de início.", nameof(dataFim));
    }

    private void ValidarValor(PromocaoTipo tipo, decimal valor)
    {
        if (tipo == PromocaoTipo.Percentual)
        {
            if (valor < 1 || valor > 100)
                throw new ArgumentException("Promoção percentual deve ter valor entre 1 e 100.", nameof(valor));
        }
        else
        {
            if (valor <= 0)
                throw new ArgumentException("Valor da promoção deve ser maior que zero.", nameof(valor));
        }
    }
}
