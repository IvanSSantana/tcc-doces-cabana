using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;

namespace DocesCabana.Application.Services;

// As invariantes de coleção (RN-01 a RN-04) moram aqui, não em Endereco: a
// entidade sozinha não conhece os irmãos, e "exatamente um principal" é
// propriedade do conjunto — mesmo limite que a 015 respeitou ao pôr a regra
// do interruptor de favorito no serviço, não em Favorito.
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

        // RN-02: o primeiro endereço cadastrado torna-se principal
        // automaticamente, sem a pessoa precisar escolher.
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

        // RN-04: excluir o principal promove outro, desde que reste algum —
        // o mais antigo entre os restantes, pela mesma ordem que a lista usa.
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

        // RN-03: marcar um endereço como principal desmarca o anterior — é
        // uma escolha entre os que existem, não um atributo independente.
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
