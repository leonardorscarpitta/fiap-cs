using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Exceptions;
using DominoPontaDeQuina.Core.Interfaces;

namespace DominoPontaDeQuina.Core.Models;

/// <inheritdoc cref="IMaoJogador"/>
public class MaoJogador(Jogador jogador) : IMaoJogador
{
    /// <summary>
    /// Mantem os lados do tabuleiro avaliados na escolha da jogada.
    /// </summary>
    private static readonly LadoTabuleiro[] LadosDisponiveis =
        [LadoTabuleiro.Esquerda, LadoTabuleiro.Direita];

    /// <summary>
    /// Obtem as pecas atualmente armazenadas na mao do jogador.
    /// </summary>
    List<Peca> _pecas = [];

    /// <inheritdoc />
    public Jogador Jogador { get; } = jogador ?? throw new ArgumentNullException(nameof(jogador));

    /// <inheritdoc />
    public void AdicionarPeca(Peca peca) => _pecas.Add(peca);

    /// <inheritdoc />
    public int SomarPecasNaMao() => _pecas.Sum(peca => peca.SomaValores);

    /// <inheritdoc />
    public bool PossuiSena() => _pecas.Any(peca => peca.EhSena);

    /// <inheritdoc />
    public bool EstaSemPecas() => _pecas.Count == 0;

    /// <summary>
    /// Determina se a mao possui alguma peca que encaixe em alguma das pontas do tabuleiro.
    /// Essa verificacao e usada na regra de travamento, pois nao altera o estado da mao.
    /// </summary>
    /// <param name="tabuleiro">O tabuleiro atual da rodada.</param>
    /// <returns><see langword="true"/> quando existir jogada possivel; caso contrario, <see langword="false"/>.</returns>
    public bool PossuiJogadaPossivel(Tabuleiro tabuleiro)
    {
        ArgumentNullException.ThrowIfNull(tabuleiro);

        return _pecas.Any(peca => LadosDisponiveis.Any(lado => tabuleiro.PodeColar(peca, lado)));
    }

    /// <inheritdoc />
    public Jogada GetJogada(Tabuleiro tabuleiro)
    {
        ArgumentNullException.ThrowIfNull(tabuleiro);

        foreach (var peca in _pecas)
        {
            foreach (var lado in LadosDisponiveis)
            {
                if (!tabuleiro.PodeColar(peca, lado))
                    continue;

                _pecas.Remove(peca);

                return new Jogada(Jogador, peca, ObterValorColado(tabuleiro, lado), lado);
            }
        }

        return new Jogada(Jogador);
    }

    /// <inheritdoc />
    public void DefazerJogada(Jogada jogada)
    {
        if (jogada is null)
            throw new JogadaInvalidaException("Nao e possivel desfazer uma jogada nula.");

        if (jogada.Peca is null)
            return;

        AdicionarPeca(jogada.Peca.Value);
    }

    /// <summary>
    /// Obtem o valor da ponta em que a peca sera encaixada.
    /// </summary>
    /// <param name="tabuleiro">O tabuleiro atual da rodada.</param>
    /// <param name="lado">O lado escolhido para a jogada.</param>
    /// <returns>O valor da ponta, ou <see langword="null"/> quando o tabuleiro estiver vazio.</returns>
    private static int? ObterValorColado(Tabuleiro tabuleiro, LadoTabuleiro lado) =>
        lado is LadoTabuleiro.Esquerda ? tabuleiro.PontaEsquerda : tabuleiro.PontaDireita;
}
