using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

public class ParticipacaoPartidaRepository
{
    private readonly DominoDbContext _context;

    public ParticipacaoPartidaRepository(DominoDbContext context)
    {
        _context = context;
    }

    public async Task<ParticipacaoPartida> AdicionarAsync(ParticipacaoPartida participacao)
    {
        _context.ParticipacoesPartida.Add(participacao);
        await _context.SaveChangesAsync();
        return participacao;
    }

    public async Task AtualizarAsync(ParticipacaoPartida participacao)
    {
        _context.ParticipacoesPartida.Update(participacao);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(ParticipacaoPartida participacao)
    {
        _context.ParticipacoesPartida.Remove(participacao);
        await _context.SaveChangesAsync();
    }

    public async Task<ParticipacaoPartida?> ObterPorIdAsync(Guid id)
    {
        return await _context.ParticipacoesPartida
            .FirstOrDefaultAsync(participacao => participacao.Id == id);
    }

    public async Task<List<ParticipacaoPartida>> ListarPorPartidaAsync(Guid partidaId)
    {
        return await _context.ParticipacoesPartida
            .Include(participacao => participacao.Jogador)
            .Where(participacao => participacao.PartidaId == partidaId)
            .OrderBy(participacao => participacao.Posicao)
            .ToListAsync();
    }

    public async Task<List<ParticipacaoPartida>> ListarPorJogadorAsync(Guid jogadorId)
    {
        return await _context.ParticipacoesPartida
            .Include(participacao => participacao.Partida)
            .Where(participacao => participacao.JogadorId == jogadorId)
            .OrderByDescending(participacao => participacao.Partida.IniciadoEm)
            .ToListAsync();
    }

    public async Task<ParticipacaoPartida?> ObterVencedorDaPartidaAsync(Guid partidaId)
    {
        return await _context.ParticipacoesPartida
            .Include(participacao => participacao.Jogador)
            .FirstOrDefaultAsync(participacao => participacao.PartidaId == partidaId && participacao.Vencedor);
    }

    public async Task<int> ContarVitoriasDoJogadorAsync(Guid jogadorId)
    {
        return await _context.ParticipacoesPartida
            .CountAsync(participacao => participacao.JogadorId == jogadorId && participacao.Vencedor);
    }

    public async Task<int> SomarPontuacaoDoJogadorAsync(Guid jogadorId)
    {
        return await _context.ParticipacoesPartida
            .Where(participacao => participacao.JogadorId == jogadorId)
            .SumAsync(participacao => participacao.Pontuacao);
    }
}
