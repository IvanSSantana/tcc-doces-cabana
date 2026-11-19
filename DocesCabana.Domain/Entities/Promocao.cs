using DocesCabana.Domain.Enums;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Domain.Entities;

public class Promocao
{
    private readonly List<Produto> _produtos = [];

    public Guid PromocaoId { get; private set; }

    public string Nome { get; private set; } = default!;

    public string? Descricao { get; private set; }

    public TipoDeDesconto Tipo { get; private set; }

    public decimal Valor { get; private set; }

    public short? QuantidadeLeve { get; private set; }

    public short? QuantidadePague { get; private set; }

    public DateOnly DataInicio { get; private set; }

    public DateOnly DataFim { get; private set; }

    public bool Ativa { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public Guid? SubcategoriaId { get; private set; }

    public Categoria? Categoria { get; private set; }

    public Subcategoria? Subcategoria { get; private set; }

    public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();

    public AbrangenciaDaPromocao Abrangencia =>
        CategoriaId is not null ? AbrangenciaDaPromocao.Categoria
        : SubcategoriaId is not null ? AbrangenciaDaPromocao.Subcategoria
        : AbrangenciaDaPromocao.ProdutosEscolhidos;

    protected Promocao() { }

    public Promocao(
        string nome,
        TipoDeDesconto tipo,
        decimal valor,
        short? quantidadeLeve,
        short? quantidadePague,
        DateOnly dataInicio,
        DateOnly dataFim,
        Guid? categoriaId,
        Guid? subcategoriaId,
        IEnumerable<Produto>? produtos,
        string? descricao = null,
        Guid id = default)
    {
        PromocaoId = id == Guid.Empty
            ? Guid.NewGuid()
            : id;

        AlterarDados(nome, descricao, tipo, valor, quantidadeLeve, quantidadePague);
        AlterarPeriodo(dataInicio, dataFim);
        AlterarAbrangencia(categoriaId, subcategoriaId, produtos);
        Ativa = true;
    }

    public void Ativar() => Ativa = true;

    public void Desativar() => Ativa = false;

    public void AlterarDados(
        string nome,
        string? descricao,
        TipoDeDesconto tipo,
        decimal valor,
        short? quantidadeLeve,
        short? quantidadePague)
    {
        ValidarNome(nome);
        ValidarDescricao(descricao);
        ValidarDesconto(tipo, valor, quantidadeLeve, quantidadePague);

        Nome = nome;
        Descricao = descricao;
        Tipo = tipo;

        if (tipo == TipoDeDesconto.CompreXLeveY)
        {
            Valor = 0;
            QuantidadeLeve = quantidadeLeve;
            QuantidadePague = quantidadePague;
        }
        else
        {
            Valor = valor;
            QuantidadeLeve = null;
            QuantidadePague = null;
        }
    }

    public void AlterarPeriodo(DateOnly dataInicio, DateOnly dataFim)
    {
        if (dataFim < dataInicio)
            throw new ArgumentException("Data de fim não pode ser anterior à data de início.", nameof(dataFim));

        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public void AlterarAbrangencia(Guid? categoriaId, Guid? subcategoriaId, IEnumerable<Produto>? produtos)
    {
        var listaDeProdutos = produtos?.ToList() ?? [];

        if (categoriaId == Guid.Empty || subcategoriaId == Guid.Empty)
            throw new ArgumentException("Categoria ou subcategoria inválida.");

        var formasEscolhidas =
            (categoriaId is not null ? 1 : 0) +
            (subcategoriaId is not null ? 1 : 0) +
            (listaDeProdutos.Count > 0 ? 1 : 0);

        if (formasEscolhidas != 1)
            throw new ArgumentException("Escolha produtos, uma categoria ou uma subcategoria para a promoção.");

        foreach (var produto in listaDeProdutos)
            ValidarProduto(produto);

        CategoriaId = categoriaId;
        SubcategoriaId = subcategoriaId;
        _produtos.Clear();
        _produtos.AddRange(listaDeProdutos.DistinctBy(p => p.ProdutoId));
    }

    public bool EstaVigente(DateOnly hoje) =>
        Ativa && hoje >= DataInicio && hoje <= DataFim;

    public bool Alcanca(Guid produtoId, Guid subcategoriaId, Guid categoriaId) => Abrangencia switch
    {
        AbrangenciaDaPromocao.Categoria => CategoriaId == categoriaId,
        AbrangenciaDaPromocao.Subcategoria => SubcategoriaId == subcategoriaId,
        _ => _produtos.Any(p => p.ProdutoId == produtoId),
    };

    public decimal CalcularDesconto(decimal precoUnitario, short quantidade) => Tipo switch
    {
        TipoDeDesconto.Percentual =>
            DescontoHelper.Arredondar(precoUnitario * quantidade * Valor / 100m),
        TipoDeDesconto.ValorFixo =>
            Valor >= precoUnitario ? 0m : DescontoHelper.Arredondar(Valor * quantidade),
        TipoDeDesconto.CompreXLeveY =>
            DescontoHelper.CalcularLeveXPagueY(precoUnitario, quantidade, QuantidadeLeve!.Value, QuantidadePague!.Value),
        _ => 0m,
    };

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException(nameof(nome), "Nome é obrigatório!");

        if (nome.Length > 255)
            throw new ArgumentException("Nome deve ter no máximo 255 caracteres.", nameof(nome));
    }

    private static void ValidarDescricao(string? descricao)
    {
        if (descricao is not null && descricao.Length > 255)
            throw new ArgumentException("Descrição deve ter no máximo 255 caracteres.", nameof(descricao));
    }

    private static void ValidarDesconto(TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague)
    {
        switch (tipo)
        {
            case TipoDeDesconto.Percentual:
                if (valor < 1 || valor > 99)
                    throw new ArgumentException("Promoção percentual deve ter valor entre 1 e 99.", nameof(valor));
                break;

            case TipoDeDesconto.ValorFixo:
                if (valor <= 0)
                    throw new ArgumentException("Valor da promoção deve ser maior que zero.", nameof(valor));
                break;

            case TipoDeDesconto.CompreXLeveY:
                if (quantidadeLeve is null || quantidadePague is null)
                    throw new ArgumentException("Informe as quantidades de \"leve\" e \"pague\".", nameof(quantidadeLeve));

                if (quantidadeLeve < 2)
                    throw new ArgumentException("A quantidade de \"leve\" deve ser no mínimo 2.", nameof(quantidadeLeve));

                if (quantidadePague < 1 || quantidadePague >= quantidadeLeve)
                    throw new ArgumentException("A quantidade de \"pague\" deve ser no mínimo 1 e menor que a de \"leve\".", nameof(quantidadePague));
                break;

            case TipoDeDesconto.FreteGratis:
                throw new ArgumentException("Frete grátis só existe como cupom.", nameof(tipo));

            default:
                throw new ArgumentException("Tipo de desconto inválido.", nameof(tipo));
        }
    }

    private static void ValidarProduto(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);

        if (produto.Status == ProdutoStatus.Inativo)
            throw new InvalidOperationException("Produto inativo não pode entrar em promoção.");

        if (produto.Status == ProdutoStatus.ForaDeEstoque)
            throw new InvalidOperationException("Produto fora de estoque não pode entrar em promoção.");
    }
}
