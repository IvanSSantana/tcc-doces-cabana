using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;

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
        var endereco = EnderecoMapper.ToEntity(dto, usuarioId);

        if (existentes.Count == 0)
            endereco.MarcarComoPadrao();

        await _enderecoRepository.Adicionar(endereco);
        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task Editar(EnderecoDTO dto, Guid usuarioId)
    {
        var endereco = await BuscarOuFalhar(dto.EnderecoId, usuarioId);

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

    private async Task<Domain.Entities.Endereco> BuscarOuFalhar(Guid enderecoId, Guid usuarioId) =>
        await _enderecoRepository.Buscar(enderecoId, usuarioId)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");
}
