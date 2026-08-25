using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Exceptions;

namespace DominoPontaDeQuina.Core.Models;

/// <summary>
/// Representa o tabuleiro no nivel da rodada dentro da hierarquia Partida -> Rodadas -> Jogadas.
/// Neste nivel ficam as pecas ja coladas e as informacoes necessarias para validar jogadas,
/// calcular pontuacao pelas pontas externas e verificar situacoes de travamento.
/// </summary>
public class Tabuleiro
{
    /// <summary>
    /// Obtem as pecas posicionadas no tabuleiro na ordem em que foram coladas.
    /// </summary>
    public List<Peca> Pecas { get; } = [];

    /// <summary>
    /// Indica se o tabuleiro ainda nao possui pecas coladas.
    /// </summary>
    public bool EstaVazio => Pecas.Count == 0;

    /// <summary>
    /// Obtem a ponta esquerda atualmente exposta no tabuleiro.
    /// Quando o tabuleiro estiver vazio, nao existe ponta externa disponivel.
    /// </summary>
    public int? PontaEsquerda => EstaVazio ? null : Pecas[0].ValorA;

    /// <summary>
    /// Obtem a ponta direita atualmente exposta no tabuleiro.
    /// Quando o tabuleiro estiver vazio, nao existe ponta externa disponivel.
    /// </summary>
    public int? PontaDireita => EstaVazio ? null : Pecas[^1].ValorB;

    /// <summary>
    /// Determina se uma peca pode ser colada no lado informado.
    /// Com o tabuleiro vazio qualquer peca e aceita; caso contrario, a peca precisa possuir
    /// o mesmo valor da ponta externa do lado escolhido.
    /// </summary>
    /// <param name="peca">A peca a ser verificada.</param>
    /// <param name="lado">O lado do tabuleiro.</param>
    /// <returns><see langword="true"/> quando a peca puder ser colada; caso contrario, <see langword="false"/>.</returns>
    public bool PodeColar(Peca peca, LadoTabuleiro lado) =>
        EstaVazio || peca.PossuiValor(ObterPonta(lado));

    /// <summary>
    /// Cola uma peca no lado informado do tabuleiro.
    /// A peca e invertida quando necessario para que o valor de encaixe fique voltado para dentro
    /// do tabuleiro e o outro valor passe a ser a nova ponta externa.
    /// </summary>
    /// <param name="peca">A peca a ser colada.</param>
    /// <param name="lado">O lado do tabuleiro.</param>
    /// <exception cref="JogadaInvalidaException">Quando a peca nao for compativel com a ponta escolhida.</exception>
    public void Colar(Peca peca, LadoTabuleiro lado)
    {
        if (!PodeColar(peca, lado))
            throw JogadaInvalidaException.PecaIncompativel(peca, lado);

        if (EstaVazio)
        {
            Pecas.Add(peca);
            return;
        }

        var ponta = ObterPonta(lado);

        if (lado is LadoTabuleiro.Esquerda)
            Pecas.Insert(0, peca.ValorB == ponta ? peca : peca.Inverter());
        else
            Pecas.Add(peca.ValorA == ponta ? peca : peca.Inverter());
    }

    /// <summary>
    /// Soma os valores das pontas externas atualmente expostas.
    /// Essa soma e a base para regras de pontuacao em que a rodada concede pontos quando o resultado for multiplo de 5.
    /// </summary>
    /// <returns>A soma das pontas externas, ou 0 quando o tabuleiro estiver vazio.</returns>
    public int SomarPontasExternas() =>
        EstaVazio ? 0 : PontaEsquerda!.Value + PontaDireita!.Value;

    /// <summary>
    /// Determina se o tabuleiro esta travado.
    /// O travamento ocorre quando nenhuma mao informada possui peca compativel com as pontas externas atuais.
    /// </summary>
    /// <param name="maosJogadores">As maos dos jogadores da rodada.</param>
    /// <returns><see langword="true"/> quando o tabuleiro estiver travado; caso contrario, <see langword="false"/>.</returns>
    public bool EstaTravado(IEnumerable<MaoJogador> maosJogadores)
    {
        if (maosJogadores is null || EstaVazio)
            return false;

        return !maosJogadores.Any(maoJogador => maoJogador.PossuiJogadaPossivel(this));
    }

    /// <summary>
    /// Limpa o tabuleiro para preparar uma nova rodada.
    /// </summary>
    public void Limpar() =>
        Pecas.Clear();

    /// <summary>
    /// Obtem o valor da ponta externa do lado informado.
    /// </summary>
    /// <param name="lado">O lado do tabuleiro.</param>
    /// <returns>O valor exposto na ponta escolhida.</returns>
    private int ObterPonta(LadoTabuleiro lado) =>
        lado is LadoTabuleiro.Esquerda ? PontaEsquerda!.Value : PontaDireita!.Value;
}
