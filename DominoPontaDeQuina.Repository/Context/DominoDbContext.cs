using DominoPontaDeQuina.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Context;

public class DominoDbContext : DbContext
{
    private const string ConnectionString = "Data Source=domino.db";

    public DbSet<Usuario> Usuarios { get; set; } = null!;

    public DbSet<Jogador> Jogadores { get; set; } = null!;

    public DbSet<Partida> Partidas { get; set; } = null!;

    public DbSet<ParticipacaoPartida> ParticipacoesPartida { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasMany(usuario => usuario.Jogadores)
            .WithOne(jogador => jogador.Usuario)
            .HasForeignKey(jogador => jogador.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Jogador>()
            .HasMany(jogador => jogador.Participacoes)
            .WithOne(participacao => participacao.Jogador)
            .HasForeignKey(participacao => participacao.JogadorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Partida>()
            .HasMany(partida => partida.Participacoes)
            .WithOne(participacao => participacao.Partida)
            .HasForeignKey(participacao => participacao.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
