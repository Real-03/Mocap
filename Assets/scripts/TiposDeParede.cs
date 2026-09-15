namespace BeatStyleGame
{
    /// <summary>
    /// Tipo de movimento que o jogador precisa de fazer para passar por uma parede.
    /// </summary>
    public enum TipoParede
    {
        Agachar,          // Parede alta — o jogador tem de se baixar
        Saltar,           // Parede baixa — o jogador tem de saltar
        DesviarEsquerda,  // Parede ocupa o lado direito — o jogador desvia para a esquerda
        DesviarDireita,   // Parede ocupa o lado esquerdo — o jogador desvia para a direita
        Forma             // Parede com uma abertura em forma específica — o jogador tem de posicionar o corpo
    }

    /// <summary>
    /// Só é usado quando o TipoParede é "Forma": define que forma o jogador deve fazer com o corpo
    /// para passar pela abertura.
    /// </summary>
    public enum FormaCorpo
    {
        Cruz,          // Braços abertos, tipo "X"
        Diagonal,      // Corpo inclinado
        Circulo,       // Corpo encolhido/pequeno
        Personalizada  // Define a tua própria forma no prefab (colliders customizados)
    }
}
