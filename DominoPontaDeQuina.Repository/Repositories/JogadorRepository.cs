using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

public class JogadorRepository
{
    private readonly DominoDbContext _context;

    public JogadorRepository(DominoDbContext context)
    {
        _context = context;
    }

    public async Task<Jogador> AdicionarAsync(Jogador jogador)
    {
        _context.Jogadores.Add(jogador);
        await _context.SaveChangesAsync();
        return jogador;
    }

    public async Task AtualizarAsync(Jogador jogador)
    {
        _context.Jogadores.Update(jogador);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Jogador jogador)
    {
        _context.Jogadores.Remove(jogador);
        await _context.SaveChangesAsync();
    }

    public async Task<Jogador?> ObterPorIdAsync(Guid id)
    {
        return await _context.Jogadores
            .Include(jogador => jogador.Usuario)
            .FirstOrDefaultAsync(jogador => jogador.Id == id);
    }

    public async Task<List<Jogador>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Jogadores
            .Where(jogador => jogador.UsuarioId == usuarioId)
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    public async Task<List<Jogador>> BuscarPorNomeExibicaoAsync(string trechoDoNome)
    {
        return await _context.Jogadores
            .Where(jogador => jogador.NomeExibicao.Contains(trechoDoNome))
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    public async Task<int> ContarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Jogadores
            .CountAsync(jogador => jogador.UsuarioId == usuarioId);
    }
}
