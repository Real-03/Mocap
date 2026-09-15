using UnityEngine;

namespace BeatStyleGame
{
    /// <summary>
    /// Tipo de objeto que pode ser gerado no jogo.
    /// </summary>
    public enum TipoObstaculo
    {
        Cubo,
        Parede
    }

    /// <summary>
    /// Colocado automaticamente (pelo SpawnManager) em cada objeto gerado.
    /// Move o objeto em direção ao jogador e destrói-o depois de passar por ele.
    /// </summary>
    public class MovingObstacle : MonoBehaviour
    {
        public TipoObstaculo tipo;

        [Tooltip("Velocidade a que o objeto se move em direção ao jogador")]
        public float velocidade = 8f;

        [Tooltip("Posição Z a partir da qual o objeto é destruído (normalmente atrás da câmara/jogador)")]
        public float distanciaDestruicao = -5f;

        private void Update()
        {
            // Move o objeto ao longo do eixo Z, no sentido do jogador
            transform.Translate(Vector3.back * velocidade * Time.deltaTime, Space.World);

            if (transform.position.z < distanciaDestruicao)
            {
                Destroy(gameObject);
            }
        }
    }
}
