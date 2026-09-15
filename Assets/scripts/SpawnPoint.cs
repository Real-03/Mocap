using System.Collections.Generic;
using UnityEngine;

namespace BeatStyleGame
{
    /// <summary>
    /// Marca uma posição onde podem ser gerados cubos ou paredes.
    /// Cria GameObjects vazios na tua cena, coloca este script neles
    /// e posiciona-os onde queres que os objetos apareçam.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Índice da faixa (0 = esquerda, 1 = centro, 2 = direita, etc.)")]
        public int indiceFaixa;

        [Tooltip("Se este ponto pode ser usado para gerar cubos")]
        public bool permiteCubos = true;

        [Tooltip("Que membros (mão/pé) podem ter cubos neste ponto. Deixa vazio para aceitar qualquer membro (desde que 'Permite Cubos' esteja ligado).")]
        public List<TipoMembro> membrosCubosSuportados = new List<TipoMembro>();

        [Tooltip("Que tipos de parede podem aparecer aqui. Deixa vazio se não houver nenhum tipo compatível (ex: um ponto só para cubos).")]
        public List<TipoParede> tiposParedeSuportados = new List<TipoParede>();

        public bool SuportaParede(TipoParede tipo)
        {
            return tiposParedeSuportados.Contains(tipo);
        }

        public bool SuportaCubo(TipoMembro membro)
        {
            if (!permiteCubos) return false;
            if (membrosCubosSuportados.Count == 0) return true; // vazio = aceita qualquer membro
            return membrosCubosSuportados.Contains(membro);
        }

        // Ajuda a visualizar os pontos de spawn na cena do editor
        private void OnDrawGizmos()
        {
            Gizmos.color = tiposParedeSuportados.Count > 0 ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
