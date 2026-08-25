namespace DominoPontaDeQuina.Core.Exceptions;

/// <summary>
/// Representa a excecao lancada quando uma operacao viola as regras da rodada.
/// Essa situacao e esperada em cenarios como iniciar a rodada sem jogadores ou organizar a fila
/// de turnos a partir de um jogador que nao participa da rodada.
/// </summary>
/// <param name="mensagem">A mensagem que descreve a regra da rodada violada.</param>
public class RodadaInvalidaException(string mensagem) : DominoException(mensagem)
{
}
