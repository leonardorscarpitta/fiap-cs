using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

public class PartidaRepository
{
    private readonly DominoDbContext _context;

    public PartidaRepository(DominoDbContext context)
    {
        _context = context;
    }

    public async Task<Partida> AdicionarAsync(Partida partida)
    {
        _context.Partidas.Add(partida);
        await _context.SaveChangesAsync();
        return partida;
    }

    public async Task AtualizarAsync(Partida partida)
    {
        _context.Partidas.Update(partida);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Partida partida)
    {
        _context.Partidas.Remove(partida);
        await _context.SaveChangesAsync();
    }

    public async Task<Partida?> ObterPorIdAsync(Guid id)
    {
        return await _context.Partidas
            .Include(partida => partida.Participacoes)
                .ThenInclude(participacao => participacao.Jogador)
            .FirstOrDefaultAsync(partida => partida.Id == id);
    }

    public async Task<List<Partida>> ListarPorStatusAsync(StatusJogo status)
    {
        return await _context.Partidas
            .Where(partida => partida.Status == status)
            .OrderByDescending(partida => partida.IniciadoEm)
            .ToListAsync();
    }

    public async Task<List<Partida>> ListarPorJogadorAsync(Guid jogadorId)
    {
        return await _context.Partidas
            .Where(partida => partida.Participacoes.Any(participacao => participacao.JogadorId == jogadorId))
            .OrderByDescending(partida => partida.IniciadoEm)
            .ToListAsync();
    }

    public async Task<List<Partida>> ListarUltimasAsync(int quantidade)
    {
        return await _context.Partidas
            .OrderByDescending(partida => partida.IniciadoEm)
            .Take(quantidade)
            .ToListAsync();
    }
}
