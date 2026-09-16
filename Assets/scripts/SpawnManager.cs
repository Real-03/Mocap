using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatStyleGame
{
    /// <summary>
    /// Gere as listas de prefabs de cubos e paredes, e gera-os ao longo do tempo
    /// respeitando uma proporção configurável (por definição, 10 cubos por cada parede).
    /// Cada prefab de parede deve ter o componente ParedeInfo a indicar o tipo
    /// (Agachar, Saltar, Desviar, Forma), para que o objeto apareça num ponto compatível.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Lista de prefabs de paredes disponíveis (cada um deve ter o componente ParedeInfo)")]
        public List<GameObject> paredesPrefabs = new List<GameObject>();

        [Tooltip("Lista de prefabs de cubos disponíveis")]
        public List<GameObject> cubosPrefabs = new List<GameObject>();

        [Header("Pontos de Spawn")]
        [Tooltip("Pontos onde os objetos podem aparecer (GameObjects com o script SpawnPoint)")]
        public List<SpawnPoint> pontosSpawn = new List<SpawnPoint>();

        [Header("Configuração de Geração")]
        [Tooltip("Quantos cubos são gerados por cada parede")]
        [Min(1)]
        public int cubosPorParede = 10;

        [Tooltip("Tempo (segundos) entre cada objeto gerado")]
        public float intervaloSpawn = 0.5f;

        [Tooltip("Tamanho de cada lote da sequência (deve ser maior que cubosPorParede + 1)")]
        public int tamanhoBlocoSequencia = 33;

        [Tooltip("Quantas vezes tenta escolher outro prefab de parede caso não haja ponto compatível com o tipo sorteado")]
        public int tentativasEscolhaParede = 5;

        [Tooltip("Quantas vezes tenta escolher outro prefab de cubo caso não haja ponto compatível com o membro-alvo sorteado")]
        public int tentativasEscolhaCubo = 5;

        [Header("Velocidade dos objetos")]
        public float velocidadeObjetos = 8f;

        [Header("Dificuldade")]

        [Range(0f, 1f)]
        [Tooltip("Probabilidade (0 a 1) de um cubo vir acompanhado de um segundo cubo simultâneo, na dificuldade Fácil")]
        public float probabilidadeCuboDuploFacil = 0f;

        [Range(0f, 1f)]
        [Tooltip("Probabilidade (0 a 1) de um cubo vir acompanhado de um segundo cubo simultâneo, na dificuldade Média")]
        public float probabilidadeCuboDuploMedio = 0.2f;

        [Range(0f, 1f)]
        [Tooltip("Probabilidade (0 a 1) de um cubo vir acompanhado de um segundo cubo simultâneo, na dificuldade Difícil")]
        public float probabilidadeCuboDuploDificil = 0.45f;

        private readonly Queue<TipoObstaculo> _sequencia = new Queue<TipoObstaculo>();
        private Coroutine _rotinaSpawn;

        private void Start()
        {
            if (pontosSpawn.Count == 0)
            {
                Debug.LogWarning("SpawnManager: não há pontos de spawn atribuídos.");
            }

            _rotinaSpawn = StartCoroutine(RotinaDeSpawn());
        }

        private IEnumerator RotinaDeSpawn()
        {
            while (true)
            {
                if (_sequencia.Count == 0)
                {
                    ConstruirNovaSequencia();
                }

                TipoObstaculo proximoTipo = _sequencia.Dequeue();
                GerarObjeto(proximoTipo);

                yield return new WaitForSeconds(intervaloSpawn);
            }
        }

        /// <summary>
        /// Cria um novo lote de objetos a gerar respeitando a proporção cubos:paredes
        /// e embaralha-os para que as paredes não caiam sempre na mesma posição da sequência.
        /// </summary>
        private void ConstruirNovaSequencia()
        {
            List<TipoObstaculo> lote = new List<TipoObstaculo>();

            int numeroParedes = Mathf.Max(1, tamanhoBlocoSequencia / (cubosPorParede + 1));
            int numeroCubos = numeroParedes * cubosPorParede;

            for (int i = 0; i < numeroParedes; i++) lote.Add(TipoObstaculo.Parede);
            for (int i = 0; i < numeroCubos; i++) lote.Add(TipoObstaculo.Cubo);

            Embaralhar(lote);

            foreach (var item in lote)
            {
                _sequencia.Enqueue(item);
            }
        }

        private void Embaralhar(List<TipoObstaculo> lista)
        {
            for (int i = lista.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (lista[i], lista[j]) = (lista[j], lista[i]);
            }
        }

        private void GerarObjeto(TipoObstaculo tipoObstaculo)
        {
            if (tipoObstaculo == TipoObstaculo.Cubo)
            {
                GerarCubo();
            }
            else
            {
                GerarParede();
            }
        }

        private void GerarCubo()
        {
            SpawnPoint primeiroPonto = GerarUmCubo(null);


        }

        /// <summary>
        /// Devolve a probabilidade de cubo duplo correspondente ao nível de dificuldade atual.
        /// </summary>
        

        /// <summary>
        /// Gera um único cubo. Se pontoAEvitar for passado (caso de um cubo duplo),
        /// garante que o segundo cubo não aparece exatamente no mesmo ponto do primeiro.
        /// Devolve o SpawnPoint usado (ou null se não foi possível gerar nada).
        /// </summary>
        private SpawnPoint GerarUmCubo(SpawnPoint pontoAEvitar)
        {
            List<GameObject> prefabsValidos = cubosPrefabs.FindAll(p => p != null);
            if (prefabsValidos.Count == 0)
            {
                Debug.LogWarning("SpawnManager: lista de cubos está vazia ou só tem referências destruídas/em falta. Confirma que arrastaste Prefabs da pasta Assets, e não objetos da Hierarchy.");
                return null;
            }

            // Tenta algumas vezes encontrar uma combinação prefab + ponto compatível,
            // porque nem todos os pontos suportam todos os membros (mão/pé).
            for (int tentativa = 0; tentativa < tentativasEscolhaCubo; tentativa++)
            {
                GameObject prefab = prefabsValidos[Random.Range(0, prefabsValidos.Count)];
                CuboInfo info = prefab.GetComponent<CuboInfo>();

                if (info == null)
                {
                    Debug.LogWarning($"SpawnManager: o prefab '{prefab.name}' não tem o componente CuboInfo.");
                    continue;
                }

                List<SpawnPoint> candidatos = pontosSpawn.FindAll(p => p.SuportaCubo(info.membroAlvo) && p != pontoAEvitar);
                if (candidatos.Count == 0) continue;

                SpawnPoint ponto = candidatos[Random.Range(0, candidatos.Count)];
                Instanciar(prefab, ponto, TipoObstaculo.Cubo);
                return ponto;
            }

            Debug.LogWarning("SpawnManager: não foi possível encontrar um ponto compatível para nenhum cubo sorteado.");
            return null;
        }

        private void GerarParede()
        {
            List<GameObject> prefabsValidos = paredesPrefabs.FindAll(p => p != null);
            if (prefabsValidos.Count == 0)
            {
                Debug.LogWarning("SpawnManager: lista de paredes está vazia ou só tem referências destruídas/em falta. Confirma que arrastaste Prefabs da pasta Assets, e não objetos da Hierarchy.");
                return;
            }

            // Tenta algumas vezes encontrar uma combinação prefab + ponto compatível,
            // porque nem todos os pontos suportam todos os tipos de parede.
            for (int tentativa = 0; tentativa < tentativasEscolhaParede; tentativa++)
            {
                GameObject prefab = prefabsValidos[Random.Range(0, prefabsValidos.Count)];
                ParedeInfo info = prefab.GetComponent<ParedeInfo>();

                if (info == null)
                {
                    Debug.LogWarning($"SpawnManager: o prefab '{prefab.name}' não tem o componente ParedeInfo.");
                    continue;
                }

                List<SpawnPoint> candidatos = pontosSpawn.FindAll(p => p.SuportaParede(info.tipo));
                if (candidatos.Count == 0) continue;

                SpawnPoint ponto = candidatos[Random.Range(0, candidatos.Count)];
                Instanciar(prefab, ponto, TipoObstaculo.Parede);
                return;
            }

            Debug.LogWarning("SpawnManager: não foi possível encontrar um ponto compatível para nenhuma parede sorteada.");
        }

        private void Instanciar(GameObject prefab, SpawnPoint ponto, TipoObstaculo tipoObstaculo)
        {
            GameObject instancia = Instantiate(prefab, ponto.transform.position, ponto.transform.rotation);

            MovingObstacle obstaculo = instancia.GetComponent<MovingObstacle>();
            if (obstaculo == null)
            {
                obstaculo = instancia.AddComponent<MovingObstacle>();
            }
            obstaculo.tipo = tipoObstaculo;
            obstaculo.velocidade = velocidadeObjetos;
        }

        /// <summary>
        /// Para a geração de objetos (por exemplo, quando a música/nível termina).
        /// </summary>
        public void PararSpawn()
        {
            if (_rotinaSpawn != null)
            {
                StopCoroutine(_rotinaSpawn);
                _rotinaSpawn = null;
            }
        }
    }
}
