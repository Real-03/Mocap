
using UnityEngine;
using UnityEngine.Events;

namespace BeatStyleGame
{
    /// <summary>
    /// Coloca este script no collider (trigger) de cada mão e de cada pé do jogador.
    /// Ao tocar num cubo, verifica se o membro deste objeto corresponde ao
    /// CuboInfo.membroAlvo do cubo — se sim, é um acerto; se não, é um erro.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TEste : MonoBehaviour
    {
        [Tooltip("Que membro este objeto representa. Ex: no collider da mão esquerda, escolhe MaoEsquerda.")]
        public TipoMembro membro;

        [Header("Eventos")]
        [Tooltip("Chamado quando este membro acerta no cubo certo. Recebe o cubo atingido.")]
        public UnityEvent<GameObject> aoAcertar;

        [Tooltip("Chamado quando este membro toca num cubo que era para outro membro. Recebe o cubo atingido.")]
        public UnityEvent<GameObject> aoErrar;

        [Tooltip("Se o cubo deve ser destruído automaticamente quando é acertado corretamente")]
        public bool destruirCuboAoAcertar = true;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Olá Unity!");
            CuboInfo cubo = other.GetComponent<CuboInfo>();
            if (cubo == null) return; // não é um cubo (ex: parede, chão), ignora

            if (cubo.membroAlvo == membro)
            {
                aoAcertar?.Invoke(other.gameObject);

                if (destruirCuboAoAcertar)
                {
                    Debug.Log("Right");
                    Destroy(other.gameObject);
                }
            }
            else
            {
                 Debug.Log("Miss");
                aoErrar?.Invoke(other.gameObject);
            }
        }
    }
}