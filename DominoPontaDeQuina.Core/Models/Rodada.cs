using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Exceptions;
using DominoPontaDeQuina.Core.Interfaces;
using System.Collections.ObjectModel;

namespace DominoPontaDeQuina.Core.Models;

/// <inheritdoc cref="IRodada"/>
public class Rodada() : IRodada
{
    /// <summary>
    /// Quantidade de pecas entregue a cada jogador no inicio da rodada.
    /// </summary>
    private const int PecasPorJogador = 7;

    /// <summary>
    /// Maior valor possivel em um dos lados de uma peca do domino tradicional.
    /// </summary>
    private const int MaiorValorDaPeca = 6;

    /// <summary>
    /// Divisor usado na regra de pontuacao das pontas externas do tabuleiro.
    /// </summary>
    private const int DivisorDaPontuacao = 5;

    /// <summary>
    /// Armazena internamente as jogadas registradas nesta rodada.
    /// </summary>
    Stack<Jogada> Jogadas { get; } = [];

    /// <inheritdoc />
    public Tabuleiro Tabuleiro { get; } = new();

    /// <summary>
    /// Mantem a fila de maos de jogadores na ordem de execucao da rodada.
    /// </summary>
    Queue<MaoJogador> _jogadores = [];

    /// <summary>
    /// Mantem a pontuacao acumulada por jogador ao longo desta rodada.
    /// </summary>
    private readonly Dictionary<Jogador, int> _pontuacaoJogadores = [];

    /// <summary>
    /// Mantem o jogador definido como vencedor no encerramento da rodada.
    /// </summary>
    private Jogador? _vencedor;

    /// <inheritdoc />
    public ReadOnlyCollection<Jogada> HistoricoJogadas => Jogadas.ToList().AsReadOnly();

    /// <inheritdoc />
    public MaoJogador JogadorAtual => _jogadores.Peek();

    /// <inheritdoc />
    public StatusRodada Status { get; private set; } = StatusRodada.NaoIniciada;

    /// <inheritdoc />
    public TipoFinalizacaoRodada? TipoFinalizacao { get; private set; }

    /// <summary>
    /// Obtem a pontuacao acumulada por jogador nesta rodada.
    /// A pontuacao e gerada quando a soma das pontas externas do tabuleiro e multipla de cinco.
    /// </summary>
    public IReadOnlyDictionary<Jogador, int> PontuacaoJogadores => _pontuacaoJogadores;

    /// <inheritdoc />
    public void Iniciar(ReadOnlyCollection<Jogador> jogadores, Rodada rodadaAnterior = null)
    {
        var maosJogadores = DistribuirPecas(jogadores);
        var primeiroJogador = GetPrimeiroJogador(maosJogadores, rodadaAnterior);
        OrganizaJogadores(maosJogadores, primeiroJogador);
        Status = StatusRodada.EmAndamento;
    }

    /// <inheritdoc />
    /// <exception cref="JogadaInvalidaException">Quando a jogada for nula ou estiver incompleta.</exception>
    public void RegistrarJogada(Jogada jogada)
    {
        if (jogada is null)
            throw new JogadaInvalidaException("Nao e possivel registrar uma jogada nula na rodada.");

        AplicarNoTabuleiro(jogada);
        jogada.MarcarComoAplicada();
        Jogadas.Push(jogada);
        CalcularPontuacao(jogada);
        PassarVez();
    }

    /// <inheritdoc />
    public bool VerificarBatida()
    {
        if (Status is not StatusRodada.EmAndamento)
            return false;

        var maoQueBateu = _jogadores.FirstOrDefault(maoJogador => maoJogador.EstaSemPecas());

        if (maoQueBateu is null)
            return false;

        Finalizar(TipoFinalizacaoRodada.JogadorBateu, maoQueBateu.Jogador);

        return true;
    }

    /// <inheritdoc />
    public bool VerificarTabuleiroTravado()
    {
        if (Status is not StatusRodada.EmAndamento || _jogadores.Count == 0)
            return false;

        if (!Tabuleiro.EstaTravado(_jogadores))
            return false;

        var maoVencedora = _jogadores
            .OrderBy(maoJogador => maoJogador.SomarPecasNaMao())
            .First();

        Finalizar(TipoFinalizacaoRodada.TabuleiroTravado, maoVencedora.Jogador);

        return true;
    }

    /// <inheritdoc />
    public Jogador? GetVencedor() => _vencedor;

    /// <summary>
    /// Distribui as pecas entre os jogadores da rodada e retorna as maos correspondentes.
    /// O tabuleiro e limpo antes da distribuicao para que a rodada comece sempre da mesa vazia.
    /// </summary>
    /// <param name="jogadores">Os jogadores participantes da rodada.</param>
    /// <returns>A lista de maos distribuidas para os jogadores.</returns>
    /// <exception cref="RodadaInvalidaException">Quando nao houver jogadores para receber as pecas.</exception>
    private List<MaoJogador> DistribuirPecas(ReadOnlyCollection<Jogador> jogadores)
    {
        if (jogadores is null || jogadores.Count == 0)
            throw new RodadaInvalidaException("Nao e possivel distribuir pecas sem jogadores na rodada.");

        Tabuleiro.Limpar();

        var pecasEmbaralhadas = GerarPecasEmbaralhadas();
        var maosJogadores = new List<MaoJogador>();
        var proximaPeca = 0;

        foreach (var jogador in jogadores)
        {
            var maoJogador = new MaoJogador(jogador);

            for (var entregues = 0; entregues < PecasPorJogador && proximaPeca < pecasEmbaralhadas.Count; entregues++)
                maoJogador.AdicionarPeca(pecasEmbaralhadas[proximaPeca++]);

            maosJogadores.Add(maoJogador);
        }

        return maosJogadores;
    }

    /// <summary>
    /// Gera o conjunto completo de pecas do domino em ordem aleatoria.
    /// </summary>
    /// <returns>As vinte e oito pecas do jogo embaralhadas.</returns>
    private static List<Peca> GerarPecasEmbaralhadas()
    {
        var pecas = new List<Peca>();

        for (var valorA = 0; valorA <= MaiorValorDaPeca; valorA++)
            for (var valorB = valorA; valorB <= MaiorValorDaPeca; valorB++)
                pecas.Add(new Peca(valorA, valorB));

        return [.. pecas.OrderBy(_ => Random.Shared.Next())];
    }

    /// <summary>
    /// Determina o primeiro jogador da rodada com base nas maos distribuidas e na rodada anterior.
    /// Na primeira rodada comeca quem possuir a sena; nas seguintes, comeca o vencedor da rodada anterior.
    /// </summary>
    /// <param name="jogadores">As maos dos jogadores desta rodada.</param>
    /// <param name="rodadaAnterior">A rodada anterior, quando houver.</param>
    /// <returns>O jogador que deve iniciar a rodada.</returns>
    private Jogador GetPrimeiroJogador(List<MaoJogador> jogadores, Rodada? rodadaAnterior = null)
    {
        if (rodadaAnterior is not null)
        {
            return rodadaAnterior.GetVencedor();
        }
        else
        {
            var maoComSena = jogadores.FirstOrDefault(maoJogador => maoJogador.PossuiSena());

            return maoComSena?.Jogador ?? jogadores[0].Jogador;
        }
    }

    /// <summary>
    /// Organiza a fila de jogadores da rodada a partir do primeiro jogador definido.
    /// A ordem original dos jogadores e preservada, apenas deslocada para comecar por quem abre a rodada.
    /// </summary>
    /// <param name="jogadores">As maos dos jogadores da rodada.</param>
    /// <param name="primeiroJogador">O jogador que iniciara a rodada.</param>
    /// <exception cref="RodadaInvalidaException">Quando o jogador informado nao participar da rodada.</exception>
    private void OrganizaJogadores(List<MaoJogador> jogadores, Jogador primeiroJogador)
    {
        var indiceInicial = jogadores.FindIndex(maoJogador => maoJogador.Jogador == primeiroJogador);

        if (indiceInicial < 0)
            throw new RodadaInvalidaException("O jogador indicado para iniciar nao participa desta rodada.");

        _jogadores.Clear();

        for (var posicao = 0; posicao < jogadores.Count; posicao++)
            _jogadores.Enqueue(jogadores[(indiceInicial + posicao) % jogadores.Count]);
    }

    /// <summary>
    /// Aplica a jogada no tabuleiro da rodada quando ela nao representar passagem de vez.
    /// </summary>
    /// <param name="jogada">A jogada a ser aplicada.</param>
    /// <exception cref="JogadaInvalidaException">Quando a jogada nao possuir peca e lado definidos.</exception>
    private void AplicarNoTabuleiro(Jogada jogada)
    {
        if (jogada.EhPassarVez())
            return;

        if (jogada.Peca is null || jogada.Lado is null)
            throw new JogadaInvalidaException("Uma jogada que nao passa a vez precisa de peca e lado definidos.");

        Tabuleiro.Colar(jogada.Peca.Value, jogada.Lado.Value);
    }

    /// <summary>
    /// Calcula a pontuacao obtida apos uma jogada ser registrada, considerando as pontas externas do tabuleiro.
    /// O jogador pontua quando a soma das pontas for multipla de cinco, e a pontuacao equivale a essa soma dividida por cinco.
    /// </summary>
    /// <param name="jogada">A jogada recem registrada na rodada.</param>
    private void CalcularPontuacao(Jogada jogada)
    {
        if (jogada.EhPassarVez() || Tabuleiro.EstaVazio)
            return;

        var somaDasPontas = Tabuleiro.SomarPontasExternas();

        if (somaDasPontas == 0 || somaDasPontas % DivisorDaPontuacao != 0)
            return;

        _pontuacaoJogadores[jogada.Jogador] =
            ObterPontuacao(jogada.Jogador) + (somaDasPontas / DivisorDaPontuacao);
    }

    /// <summary>
    /// Obtem a pontuacao ja acumulada pelo jogador nesta rodada.
    /// </summary>
    /// <param name="jogador">O jogador consultado.</param>
    /// <returns>A pontuacao acumulada, ou zero quando o jogador ainda nao tiver pontuado.</returns>
    private int ObterPontuacao(Jogador jogador) =>
        _pontuacaoJogadores.TryGetValue(jogador, out var pontuacao) ? pontuacao : 0;

    /// <summary>
    /// Passa a vez para o proximo jogador da fila, mantendo o sentido horario da rodada.
    /// </summary>
    private void PassarVez()
    {
        if (_jogadores.Count == 0)
            return;

        _jogadores.Enqueue(_jogadores.Dequeue());
    }

    /// <summary>
    /// Encerra a rodada registrando o motivo da finalizacao e o jogador vencedor.
    /// </summary>
    /// <param name="tipoFinalizacao">O motivo do encerramento da rodada.</param>
    /// <param name="vencedor">O jogador vencedor da rodada.</param>
    private void Finalizar(TipoFinalizacaoRodada tipoFinalizacao, Jogador vencedor)
    {
        _vencedor = vencedor;
        TipoFinalizacao = tipoFinalizacao;
        Status = StatusRodada.Finalizada;
    }
}
