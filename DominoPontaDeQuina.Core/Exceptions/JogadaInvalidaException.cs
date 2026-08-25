using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Models;

namespace DominoPontaDeQuina.Core.Exceptions;

/// <summary>
/// Representa a excecao lancada quando uma jogada nao respeita as regras do jogo.
/// Essa situacao e esperada quando a jogada e nula ou quando a peca escolhida nao e compativel
/// com a ponta do lado indicado no tabuleiro.
/// </summary>
/// <param name="mensagem">A mensagem que descreve o motivo da jogada ser invalida.</param>
public class JogadaInvalidaException(string mensagem) : DominoException(mensagem)
{
    /// <summary>
    /// Cria a excecao para o caso em que a peca nao encaixa na ponta escolhida.
    /// </summary>
    /// <param name="peca">A peca que se tentou colar.</param>
    /// <param name="lado">O lado do tabuleiro escolhido para a jogada.</param>
    /// <returns>A excecao com a mensagem detalhando a incompatibilidade.</returns>
    public static JogadaInvalidaException PecaIncompativel(Peca peca, LadoTabuleiro lado) =>
        new($"A peca {peca} nao e compativel com a ponta {lado} do tabuleiro.");
}
