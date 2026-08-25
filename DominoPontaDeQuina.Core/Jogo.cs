using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Exceptions;
using DominoPontaDeQuina.Core.Models;
using System.Collections.ObjectModel;

namespace DominoPontaDeQuina.Core;

/// <summary>
/// Controla o fluxo principal no topo da hierarquia Partida -> Rodadas -> Jogadas.
/// Neste nivel fica a orquestracao da partida atual, da sequencia de rodadas e da execucao das jogadas.
/// </summary>
public class Jogo()
{
    /// <summary>
    /// Mantem internamente o historico das partidas iniciadas pelo jogo.
    /// </summary>
    Stack<Partida> _partidas = [];

    /// <summary>
    /// Obtem o historico das partidas controladas por esta instancia.
    /// </summary>
    public ReadOnlyCollection<Partida> HistoricoPartidas => _partidas.ToList().AsReadOnly();

    /// <summary>
    /// Obtem a partida atual controlada pelo jogo.
    /// </summary>
    public Partida? PartidaAtual => _partidas.TryPeek(out var partidaAtual) ? partidaAtual : null;

    /// <summary>
    /// Registra os times da partida atual.
    /// A partida e organizada em dois times com dois jogadores cada, formato em que a composicao
    /// dos times define a disputa em duplas.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando nao houver partida atual para receber os times.</exception>
    public Task RegistrarTimesAsync()
    {
        if (PartidaAtual is null)
            throw new PartidaInvalidaException("Nao ha partida atual para registrar os times.");

        if (PartidaAtual.Times.Count > 0)
            return Task.CompletedTask;

        var primeiroTime = new Time("Time A");
        primeiroTime.AdicionarJogador(new Jogador("Jogador 1"));
        primeiroTime.AdicionarJogador(new Jogador("Jogador 3"));

        var segundoTime = new Time("Time B");
        segundoTime.AdicionarJogador(new Jogador("Jogador 2"));
        segundoTime.AdicionarJogador(new Jogador("Jogador 4"));

        PartidaAtual.Times.Add(primeiroTime);
        PartidaAtual.Times.Add(segundoTime);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Inicia uma nova partida e executa suas rodadas ate a finalizacao.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando a partida atual ainda estiver em andamento.</exception>
    public async Task IniciarNovaPartida()
    {
        if (PartidaAtual?.Status is StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel iniciar uma nova partida enquanto a partida atual estiver em andamento.");

        _partidas.Push(new());

        await RegistrarTimesAsync();

        PartidaAtual.IniciarNovaRodada();
        PartidaAtual.RodadaAtual?.Iniciar(ObterJogadoresDaPartida(), ObterRodadaAnterior());

        while (PartidaAtual.Status is StatusPartida.EmAndamento)
        {
            await ExecutarRodadaPartidaAsync();

            if (PartidaAtual.VerificaPontuacaoAlvoAtingida())
            {
                PartidaAtual.FinalizarPartida();
            }
            else
            {
                PartidaAtual.IniciarNovaRodada();
                PartidaAtual.RodadaAtual?.Iniciar(ObterJogadoresDaPartida(), ObterRodadaAnterior());
            }
        }
    }

    /// <summary>
    /// Executa o fluxo da rodada atual enquanto ela estiver em andamento.
    /// </summary>
    public async Task ExecutarRodadaPartidaAsync()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            return;
        if (PartidaAtual.RodadaAtual?.Status is not StatusRodada.EmAndamento)
            return;

        var rodadaAtual = PartidaAtual.RodadaAtual;

        while (rodadaAtual.Status is StatusRodada.EmAndamento)
        {
            await ExecutarJogadaAsync();
            rodadaAtual.VerificarBatida();
            rodadaAtual.VerificarTabuleiroTravado();
        }
    }

    /// <summary>
    /// Executa a jogada do jogador atual na rodada em andamento.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando a partida nao estiver em andamento.</exception>
    /// <exception cref="RodadaInvalidaException">Quando a rodada nao estiver em andamento.</exception>
    /// <exception cref="JogadaInvalidaException">Quando a jogada escolhida nao for valida.</exception>
    public async Task ExecutarJogadaAsync()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel executar uma jogada em uma partida que nao esta em andamento.");
        if (PartidaAtual.RodadaAtual?.Status is not StatusRodada.EmAndamento)
            throw new RodadaInvalidaException("Nao e possivel executar uma jogada em uma rodada que nao esta em andamento.");

        var jogadorAtual = PartidaAtual.RodadaAtual.JogadorAtual;
        var jogada = await GetJogadaAsync();

        if (!ValidarJogada(jogada))
        {
            jogadorAtual.DefazerJogada(jogada);
            jogada.MarcarComoInvalida();
            throw new JogadaInvalidaException("A jogada realizada e invalida.");
        }

        PartidaAtual.RodadaAtual.RegistrarJogada(jogada);
    }

    /// <summary>
    /// Obtem a jogada definida pelo jogador atual com base no estado do tabuleiro.
    /// </summary>
    /// <returns>A jogada escolhida pelo jogador atual.</returns>
    /// <exception cref="RodadaInvalidaException">Quando nao houver rodada atual na partida.</exception>
    public Task<Jogada> GetJogadaAsync()
    {
        if (PartidaAtual?.RodadaAtual is null)
            throw new RodadaInvalidaException("Nao ha rodada atual para obter jogada.");

        var jogadorAtual = PartidaAtual.RodadaAtual.JogadorAtual;
        return Task.FromResult(jogadorAtual.GetJogada(PartidaAtual.RodadaAtual.Tabuleiro));
    }

    /// <summary>
    /// Valida a jogada no contexto da rodada atual.
    /// Passar a vez e sempre valido; nas demais jogadas a peca precisa encaixar na ponta escolhida.
    /// </summary>
    /// <param name="jogada">A jogada a ser validada.</param>
    /// <returns><see langword="true"/> quando a jogada for valida; caso contrario, <see langword="false"/>.</returns>
    public bool ValidarJogada(Jogada jogada)
    {
        if (jogada is null)
            return false;

        if (jogada.EhPassarVez())
            return true;

        if (jogada.Peca is null || jogada.Lado is null)
            return false;

        var tabuleiro = PartidaAtual?.RodadaAtual?.Tabuleiro;

        return tabuleiro is not null && tabuleiro.PodeColar(jogada.Peca.Value, jogada.Lado.Value);
    }

    /// <summary>
    /// Obtem os jogadores registrados nos times da partida atual.
    /// </summary>
    /// <returns>A colecao somente leitura dos jogadores da partida.</returns>
    /// <exception cref="PartidaInvalidaException">Quando nao houver partida atual.</exception>
    private ReadOnlyCollection<Jogador> ObterJogadoresDaPartida()
    {
        if (PartidaAtual is null)
            throw new PartidaInvalidaException("Nao ha partida atual para obter jogadores.");

        return PartidaAtual.Times
            .SelectMany(time => time.Jogadores)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Obtem a rodada anterior a rodada atual, quando houver.
    /// </summary>
    /// <returns>A rodada anterior, ou <see langword="null"/> quando a rodada atual for a primeira da partida.</returns>
    private Rodada? ObterRodadaAnterior()
    {
        if (PartidaAtual is null || PartidaAtual.HistoricoRodadas.Count < 2)
            return null;

        return PartidaAtual.HistoricoRodadas[1];
    }
}
