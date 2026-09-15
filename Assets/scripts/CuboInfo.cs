using UnityEngine;

namespace BeatStyleGame
{
    /// <summary>
    /// Coloca este script em cada PREFAB de cubo, para indicar com que mão ou pé
    /// o jogador deve interagir com ele.
    /// </summary>
    public class CuboInfo : MonoBehaviour
    {
        [Tooltip("Membro do corpo que deve interagir com este cubo")]
        public TipoMembro membroAlvo;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = CorPorMembro(membroAlvo);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        private Color CorPorMembro(TipoMembro membro)
        {
            switch (membro)
            {
                case TipoMembro.MaoEsquerda: return Color.blue;
                case TipoMembro.MaoDireita: return Color.red;
                case TipoMembro.PeEsquerdo: return Color.cyan;
                case TipoMembro.PeDireito: return new Color(1f, 0.5f, 0f); // laranja
                default: return Color.gray;
            }
        }
    }
}
