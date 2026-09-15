using UnityEngine;

namespace BeatStyleGame
{
    /// <summary>
    /// Coloca este script em cada PREFAB de parede, para dizer ao SpawnManager
    /// que tipo de movimento o jogador tem de fazer para passar por ela.
    /// </summary>
    public class ParedeInfo : MonoBehaviour
    {
        [Tooltip("Tipo de movimento que o jogador precisa de fazer para passar por esta parede")]
        public TipoParede tipo;

        [Tooltip("Só relevante se o tipo for 'Forma': que forma o jogador deve fazer com o corpo")]
        public FormaCorpo forma = FormaCorpo.Personalizada;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = CorPorTipo(tipo);
            Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 2f, 0.3f));
        }

        private Color CorPorTipo(TipoParede t)
        {
            switch (t)
            {
                case TipoParede.Agachar: return Color.yellow;
                case TipoParede.Saltar: return Color.green;
                case TipoParede.DesviarEsquerda: return Color.blue;
                case TipoParede.DesviarDireita: return Color.magenta;
                case TipoParede.Forma: return Color.white;
                default: return Color.gray;
            }
        }
    }
}
