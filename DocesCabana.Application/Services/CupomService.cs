using System.Globalization;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Services;

public class CupomService : ICupomService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ICupomRepository _cupomRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ISubcategoriaRepository _subcategoriaRepository;
    private readonly TimeProvider _relogio;
    private readonly IUnitOfWork _unitOfWork;

    public CupomService(
        ICupomRepository cupomRepository, IPedidoRepository pedidoRepository,
        ISubcategoriaRepository subcategoriaRepository, TimeProvider relogio, IUnitOfWork unitOfWork)
    {
        _cupomRepository = cupomRepository;
        _pedidoRepository = pedidoRepository;
        _subcategoriaRepository = subcategoriaRepository;
        _relogio = relogio;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultadoDoCupomDTO> Avaliar(
        string codigo, Guid usuarioId, CarrinhoDTO carrinho, decimal? valorDoFrete,
        ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega)
    {
        var normalizado = Cupom.NormalizarCodigo(codigo);
        var cupom = await _cupomRepository.BuscarPorCodigo(normalizado);

        if (cupom is null || !cupom.Ativo)
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, "Cupom não encontrado.");

        var rotulo = CupomMapper.Rotulo(cupom);
        var hoje = HorarioDeBrasilia.Hoje(_relogio);

        if (hoje < cupom.DataInicio)
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, "Este cupom ainda não está valendo.", rotulo);

        if (hoje > cupom.DataFim)
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, "Este cupom expirou.", rotulo);

        if (cupom.Tipo == TipoDeDesconto.FreteGratis && modalidade == ModalidadeDeEntrega.RetiradaNaLoja)
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, "Cupom de frete grátis não vale para retirada na loja.", rotulo);

        if (cupom.UmUsoPorCliente && await _pedidoRepository.ClienteJaUsouCupom(usuarioId, cupom.CupomId))
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, "Você já usou este cupom.", rotulo);

        var elegiveis = await LinhasElegiveis(cupom, carrinho);
        var nomeDaRestricao = CupomMapper.NomeDaRestricao(cupom);

        if (nomeDaRestricao is not null && elegiveis.Count == 0)
            return ResultadoDoCupomDTO.ParaRecusa(normalizado, $"Este cupom vale só para itens de {nomeDaRestricao}.", rotulo);

        var baseElegivel = elegiveis.Sum(l => l.ValorDaLinha);

        if (!cupom.AtingeMinimo(baseElegivel))
        {
            var emRestricao = nomeDaRestricao is null ? string.Empty : $" em {nomeDaRestricao}";
            return ResultadoDoCupomDTO.ParaRecusa(normalizado,
                $"Este cupom vale para compras a partir de R$ {cupom.ValorMinimo!.Value.ToString("N2", PtBr)}{emRestricao}.",
                rotulo);
        }

        if (cupom.Tipo == TipoDeDesconto.CompreXLeveY && elegiveis.All(l => l.Quantidade < cupom.QuantidadeLeve))
            return ResultadoDoCupomDTO.ParaRecusa(normalizado,
                $"Leve {cupom.QuantidadeLeve} unidades de um mesmo produto para usar este cupom.", rotulo);

        var linhas = elegiveis.Select(l => (l.ValorDaLinha, l.Quantidade)).ToList();

        return new ResultadoDoCupomDTO
        {
            Codigo = normalizado,
            CupomId = cupom.CupomId,
            Valido = true,
            Rotulo = rotulo,
            FreteGratis = cupom.Tipo == TipoDeDesconto.FreteGratis,
            DescontoNosProdutos = cupom.CalcularDescontoNosProdutos(linhas),
            DescontoNoFrete = valorDoFrete is null ? 0m : cupom.CalcularDescontoNoFrete(valorDoFrete.Value)
        };
    }

    public async Task<bool> CodigoJaExiste(string codigo) =>
        await _cupomRepository.ExisteCodigo(Cupom.NormalizarCodigo(codigo));

    public async Task<IReadOnlyList<ResumoDeCupomDTO>> ListarParaGestao()
    {
        var hoje = HorarioDeBrasilia.Hoje(_relogio);
        var cupons = await _cupomRepository.ListarParaGestao();
        var usos = await _pedidoRepository.ContarUsosPorCupom();

        return cupons
            .Select(c => CupomMapper.ToResumoDTO(c, hoje, usos.GetValueOrDefault(c.CupomId)))
            .ToList();
    }

    public async Task<CupomDTO> BuscarParaEdicao(Guid cupomId) =>
        CupomMapper.ToDTO(await BuscarObrigatorio(cupomId));

    public async Task Cadastrar(CupomDTO dto)
    {
        if (await CodigoJaExiste(dto.Codigo))
            throw new InvalidOperationException("Já existe um cupom com este código.");

        var cupom = new Cupom(
            dto.Codigo,
            dto.Tipo,
            dto.Valor ?? 0,
            dto.QuantidadeLeve,
            dto.QuantidadePague,
            dto.DataInicio!.Value,
            dto.DataFim!.Value,
            dto.ValorMinimo,
            dto.UmUsoPorCliente,
            dto.CategoriaId,
            dto.SubcategoriaId,
            dto.Descricao);

        await _cupomRepository.Adicionar(cupom);
        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Atualizar(CupomDTO dto)
    {
        var cupom = await BuscarObrigatorio(dto.CupomId);

        cupom.AlterarDados(dto.Tipo, dto.Valor ?? 0, dto.QuantidadeLeve, dto.QuantidadePague, dto.Descricao);
        cupom.AlterarPeriodo(dto.DataInicio!.Value, dto.DataFim!.Value);
        cupom.AlterarRestricoes(dto.ValorMinimo, dto.UmUsoPorCliente, dto.CategoriaId, dto.SubcategoriaId);

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Ativar(Guid cupomId)
    {
        var cupom = await BuscarObrigatorio(cupomId);

        cupom.Ativar();

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Desativar(Guid cupomId)
    {
        var cupom = await BuscarObrigatorio(cupomId);

        cupom.Desativar();

        await _unitOfWork.SalvarAlteracoes();
    }

    private async Task<Cupom> BuscarObrigatorio(Guid cupomId) =>
        await _cupomRepository.BuscarPorId(cupomId)
            ?? throw new KeyNotFoundException($"Cupom com ID {cupomId} não encontrado.");

    private async Task<List<LinhaDoCarrinhoDTO>> LinhasElegiveis(Cupom cupom, CarrinhoDTO carrinho)
    {
        var disponiveis = carrinho.Linhas.Where(l => l.Disponivel).ToList();

        if (cupom.CategoriaId is null && cupom.SubcategoriaId is null)
            return disponiveis;

        var subcategorias = await _subcategoriaRepository.BuscarTodos();
        var categoriaPorSubcategoria = subcategorias.ToDictionary(s => s.SubcategoriaId, s => s.CategoriaId);

        return disponiveis
            .Where(l => cupom.Alcanca(
                l.SubcategoriaId,
                categoriaPorSubcategoria.TryGetValue(l.SubcategoriaId, out var categoriaId) ? categoriaId : Guid.Empty))
            .ToList();
    }
}
