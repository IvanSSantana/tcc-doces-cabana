using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class EnderecoRepository : IEnderecoRepository
{
    private readonly DocesCabanaDbContext _context;

    public EnderecoRepository(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Endereco>> BuscarPorUsuario(Guid usuarioId) =>
        await _context.Enderecos
            .Where(e => e.UsuarioId == usuarioId)
            .OrderBy(e => e.DataCadastro)
            .ToListAsync();

    public async Task<Endereco?> Buscar(Guid enderecoId, Guid usuarioId) =>
        await _context.Enderecos
            .FirstOrDefaultAsync(e => e.EnderecoId == enderecoId && e.UsuarioId == usuarioId);

    public async Task Adicionar(Endereco endereco) =>
        await _context.Enderecos.AddAsync(endereco);

    public void Remover(Endereco endereco) =>
        _context.Enderecos.Remove(endereco);
}
