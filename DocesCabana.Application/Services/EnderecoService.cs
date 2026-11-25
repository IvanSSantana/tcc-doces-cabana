using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Services;

public class EnderecoService : IEnderecoService
{
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnderecoService(IEnderecoRepository enderecoRepository, IUnitOfWork unitOfWork)
    {
        _enderecoRepository = enderecoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EnderecoDTO>> ListarDoUsuario(Guid usuarioId)
    {
        var enderecos = await _enderecoRepository.BuscarPorUsuario(usuarioId);
        return EnderecoMapper.ToDTO(enderecos);
    }

    public async Task<EnderecoDTO> BuscarDoUsuario(Guid enderecoId, Guid usuarioId)
    {
        var endereco = await BuscarOuFalhar(enderecoId, usuarioId);
        return EnderecoMapper.ToDTO(endereco);
    }

    public async Task Cadastrar(EnderecoDTO dto, Guid usuarioId)
    {
        var existentes = await _enderecoRepository.BuscarPorUsuario(usuarioId);
        if (HaRepetido(existentes, dto))
            throw new InvalidOperationException(MensagensEndereco.EnderecoRepetido);

        var endereco = EnderecoMapper.ToEntity(dto, usuarioId);

        if (existentes.Count == 0)
            endereco.MarcarComoPadrao();

        await _enderecoRepository.Adicionar(endereco);
        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Editar(EnderecoDTO dto, Guid usuarioId)
    {
        var endereco = await BuscarOuFalhar(dto.EnderecoId, usuarioId);

        if (await EnderecoJaCadastrado(dto, usuarioId))
            throw new InvalidOperationException(MensagensEndereco.EnderecoRepetido);

        endereco.AtualizarDados(dto.Estado, dto.Cidade, dto.Bairro, dto.CEP, dto.Rua, dto.Numero, dto.Complemento);

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Excluir(Guid enderecoId, Guid usuarioId)
    {
        var enderecos = await _enderecoRepository.BuscarPorUsuario(usuarioId);
        var endereco = enderecos.FirstOrDefault(e => e.EnderecoId == enderecoId)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");

        var eraPrincipal = endereco.Padrao;
        _enderecoRepository.Remover(endereco);

        if (eraPrincipal)
        {
            var proximo = enderecos
                .Where(e => e.EnderecoId != enderecoId)
                .OrderBy(e => e.DataCadastro)
                .FirstOrDefault();

            proximo?.MarcarComoPadrao();
        }

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task TornarPrincipal(Guid enderecoId, Guid usuarioId)
    {
        var endereco = await BuscarOuFalhar(enderecoId, usuarioId);

        var enderecos = await _enderecoRepository.BuscarPorUsuario(usuarioId);
        var anteriorPrincipal = enderecos.FirstOrDefault(e => e.Padrao && e.EnderecoId != enderecoId);
        anteriorPrincipal?.DesmarcarComoPadrao();

        endereco.MarcarComoPadrao();

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task<bool> EnderecoJaCadastrado(EnderecoDTO dto, Guid usuarioId) =>
        HaRepetido(await _enderecoRepository.BuscarPorUsuario(usuarioId), dto);

    /// <summary>
    /// Mesmo CEP (só dígitos), número e complemento de outro endereço da conta. O complemento ignora
    /// caixa e espaços nas pontas, e vazio vale o mesmo que nulo (spec 039, D12). Na edição, o
    /// próprio endereço não conta.
    /// </summary>
    private static bool HaRepetido(IEnumerable<Domain.Entities.Endereco> existentes, EnderecoDTO dto)
    {
        var cep = CepHelper.ApenasDigitos(dto.CEP ?? string.Empty);
        var complemento = NormalizarComplemento(dto.Complemento);

        return existentes.Any(e =>
            e.EnderecoId != dto.EnderecoId &&
            e.CEP == cep &&
            e.Numero == dto.Numero &&
            NormalizarComplemento(e.Complemento) == complemento);
    }

    private static string NormalizarComplemento(string? complemento) =>
        (complemento ?? string.Empty).Trim().ToUpperInvariant();

    private async Task<Domain.Entities.Endereco> BuscarOuFalhar(Guid enderecoId, Guid usuarioId) =>
        await _enderecoRepository.Buscar(enderecoId, usuarioId)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");
}
