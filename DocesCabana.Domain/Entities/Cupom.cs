using System.Text.RegularExpressions;
using DocesCabana.Domain.Enums;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public partial class Cupom
{
    public const int TamanhoMaximoDoCodigo = 20;

    public Guid CupomId { get; private set; }

    public string Codigo { get; private set; } = default!;

    public string? Descricao { get; private set; }

    public TipoDeDesconto Tipo { get; private set; }

    public decimal Valor { get; private set; }

    public short? QuantidadeLeve { get; private set; }

    public short? QuantidadePague { get; private set; }

    public DateOnly DataInicio { get; private set; }

    public DateOnly DataFim { get; private set; }

    public decimal? ValorMinimo { get; private set; }

    public bool UmUsoPorCliente { get; private set; }

    public bool Ativo { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public Guid? SubcategoriaId { get; private set; }

    public Categoria? Categoria { get; private set; }

    public Subcategoria? Subcategoria { get; private set; }

    protected Cupom() { }

    public Cupom(
        string codigo,
        TipoDeDesconto tipo,
        decimal valor,
        short? quantidadeLeve,
        short? quantidadePague,
        DateOnly dataInicio,
        DateOnly dataFim,
        decimal? valorMinimo,
        bool umUsoPorCliente,
        Guid? categoriaId,
        Guid? subcategoriaId,
        string? descricao = null,
        Guid id = default)
    {
        CupomId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        Codigo = ValidarCodigo(codigo);
        AlterarDados(tipo, valor, quantidadeLeve, quantidadePague, descricao);
        AlterarPeriodo(dataInicio, dataFim);
        AlterarRestricoes(valorMinimo, umUsoPorCliente, categoriaId, subcategoriaId);
        Ativo = true;
    }

    public static string NormalizarCodigo(string codigo) =>
        (codigo ?? string.Empty).Trim().ToUpperInvariant();

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public void AlterarDados(
        TipoDeDesconto tipo,
        decimal valor,
        short? quantidadeLeve,
        short? quantidadePague,
        string? descricao)
    {
        DescontoHelper.ValidarTipoEValor(tipo, valor, quantidadeLeve, quantidadePague);
        ValidarDescricao(descricao);

        Tipo = tipo;
        Descricao = descricao;

        switch (tipo)
        {
            case TipoDeDesconto.CompreXLeveY:
                Valor = 0;
                QuantidadeLeve = quantidadeLeve;
                QuantidadePague = quantidadePague;
                break;

            case TipoDeDesconto.FreteGratis:
                Valor = 0;
                QuantidadeLeve = null;
                QuantidadePague = null;
                break;

            default:
                Valor = valor;
                QuantidadeLeve = null;
                QuantidadePague = null;
                break;
        }
    }

    public void AlterarPeriodo(DateOnly dataInicio, DateOnly dataFim)
    {
        if (dataFim < dataInicio)
            throw new ArgumentException("Data de fim não pode ser anterior à data de início.", nameof(dataFim));

        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public void AlterarRestricoes(decimal? valorMinimo, bool umUsoPorCliente, Guid? categoriaId, Guid? subcategoriaId)
    {
        if (valorMinimo is not null && valorMinimo <= 0)
            throw new ArgumentException("Valor mínimo deve ser maior que zero.", nameof(valorMinimo));

        if (categoriaId == Guid.Empty || subcategoriaId == Guid.Empty)
            throw new ArgumentException("Categoria ou subcategoria inválida.");

        if (categoriaId is not null && subcategoriaId is not null)
            throw new ArgumentException("Restrinja o cupom a uma categoria ou a uma subcategoria, não às duas.");

        ValorMinimo = valorMinimo;
        UmUsoPorCliente = umUsoPorCliente;
        CategoriaId = categoriaId;
        SubcategoriaId = subcategoriaId;
    }

    public bool EstaVigente(DateOnly hoje) =>
        Ativo && hoje >= DataInicio && hoje <= DataFim;

    public bool Alcanca(Guid subcategoriaId, Guid categoriaId)
    {
        if (CategoriaId is not null)
            return CategoriaId == categoriaId;

        if (SubcategoriaId is not null)
            return SubcategoriaId == subcategoriaId;

        return true;
    }

    public bool AtingeMinimo(decimal baseElegivel) =>
        ValorMinimo is null || baseElegivel >= ValorMinimo;

    public decimal CalcularDescontoNosProdutos(IReadOnlyList<(decimal ValorLiquido, short Quantidade)> linhasElegiveis)
    {
        var baseElegivel = linhasElegiveis.Sum(l => l.ValorLiquido);

        return Tipo switch
        {
            TipoDeDesconto.Percentual =>
                DescontoHelper.Arredondar(baseElegivel * Valor / 100m),
            TipoDeDesconto.ValorFixo =>
                Math.Min(Valor, baseElegivel),
            TipoDeDesconto.CompreXLeveY =>
                linhasElegiveis.Sum(l => DescontoHelper.CalcularLeveXPagueY(
                    l.ValorLiquido / l.Quantidade, l.Quantidade, QuantidadeLeve!.Value, QuantidadePague!.Value)),
            _ => 0m,
        };
    }

    public decimal CalcularDescontoNoFrete(decimal valorDoFrete) =>
        Tipo == TipoDeDesconto.FreteGratis ? valorDoFrete : 0m;

    private static string ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentNullException(nameof(codigo), "Código é obrigatório!");

        var normalizado = NormalizarCodigo(codigo);

        if (!FormatoDoCodigo().IsMatch(normalizado))
            throw new ArgumentException("O código deve ter de 3 a 20 letras ou números, sem espaços.", nameof(codigo));

        return normalizado;
    }

    private static void ValidarDescricao(string? descricao)
    {
        if (descricao is not null && descricao.Length > 255)
            throw new ArgumentException("Descrição deve ter no máximo 255 caracteres.", nameof(descricao));
    }

    [GeneratedRegex("^[A-Z0-9]{3,20}$")]
    private static partial Regex FormatoDoCodigo();
}
