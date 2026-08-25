namespace DominoPontaDeQuina.Core.Exceptions;

/// <summary>
/// Representa a excecao lancada quando uma operacao viola as regras da partida.
/// Essa situacao e esperada em cenarios como finalizar uma partida que nao esta em andamento
/// ou iniciar uma nova rodada em uma partida ja finalizada.
/// </summary>
/// <param name="mensagem">A mensagem que descreve a regra da partida violada.</param>
public class PartidaInvalidaException(string mensagem) : DominoException(mensagem)
{
}
