namespace DominoPontaDeQuina.Core.Exceptions;

/// <summary>
/// Representa a excecao base do dominio do jogo de domino.
/// Todas as excecoes especificas das regras do jogo derivam desta classe, permitindo que o fluxo
/// diferencie falhas de dominio de falhas tecnicas da plataforma.
/// </summary>
/// <param name="mensagem">A mensagem que descreve a regra de dominio violada.</param>
public abstract class DominoException(string mensagem) : Exception(mensagem)
{
}
