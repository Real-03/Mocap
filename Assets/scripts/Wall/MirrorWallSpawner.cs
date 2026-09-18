using UnityEngine;
using System.Collections;

/// <summary>
/// Gera paredes/obstáculos periodicamente num ponto de spawn, manda-as mover-se
/// em direção ao jogador (via MirrorWallMover) e liga automaticamente cada uma
/// ao MirrorChallengeResultUI, sem precisares de configurar nada à mão por parede.
///
/// Suporta múltiplos prefabs de parede — cada spawn escolhe um deles.
///
/// O prefab da parede só precisa de ter, ele próprio:
/// - um Collider (Mesh, Box, etc.) com a forma/abertura do desafio;
/// - o MirrorWallDetector (auto-configura-se sozinho, como já sabes);
/// - o MirrorWallMover (controla o movimento desta parede específica).
/// </summary>
public class MirrorWallSpawner : MonoBehaviour
{
    private enum PrefabSelectionMode
    {
        Random,
        Sequential
    }

    [Header("Prefabs e spawn")]
    [Tooltip("Lista de prefabs de parede possíveis. Cada um tem de ter MirrorWallMover (e, por causa do RequireComponent, também já vem com MirrorWallDetector e um Collider).")]
    [SerializeField] private MirrorWallMover[] wallPrefabs;
    [Tooltip("Como escolher o prefab a cada spawn: Random (aleatório) ou Sequential (segue a ordem da lista e repete do início).")]
    [SerializeField] private PrefabSelectionMode selectionMode = PrefabSelectionMode.Random;
    [Tooltip("Se ativo, evita repetir o mesmo prefab duas vezes seguidas quando o modo é Random (ignorado no modo Sequential).")]
    [SerializeField] private bool avoidImmediateRepeatOnRandom = true;

    [Tooltip("Onde cada parede nova aparece. A rotação do spawn point também é aplicada à parede.")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 4f;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Se ativo, calcula a direção automaticamente do spawn point até à posição atual do jogador. Se desativado, usa sempre Manual Move Direction.")]
    [SerializeField] private bool autoDirectionTowardsPlayer = true;
    [Tooltip("Usado só se Auto Direction estiver desligado.")]
    [SerializeField] private Vector3 manualMoveDirection = Vector3.back;
    [Tooltip("Ignora a diferença de altura (eixo Y) ao calcular a direção automática — normalmente queres que a parede se mova só no plano horizontal.")]
    [SerializeField] private bool ignoreHeightDifference = true;

    [Header("Integração (opcional)")]
    [Tooltip("Se atribuído, cada parede nova é automaticamente ligada a esta UI (SetDetector), e o veredito é mostrado no momento em que a parede cruza o jogador.")]
    [SerializeField] private MirrorChallengeResultUI resultUI;

    private Coroutine spawnLoop;
    private int nextSequentialIndex = 0;
    private int lastRandomIndex = -1;

    private void OnEnable()
    {
        spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnLoop != null) StopCoroutine(spawnLoop);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Gera uma parede imediatamente, escolhendo um prefab da lista. Podes chamar
    /// isto manualmente (ex.: para o primeiro desafio começar já) em vez de
    /// esperares pelo primeiro intervalo.
    /// </summary>
    public void SpawnWall()
    {
        MirrorWallMover prefabToSpawn = PickPrefab();

        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning("[MirrorWallSpawner] Nenhum Wall Prefab válido ou Spawn Point não atribuído.", this);
            return;
        }

        MirrorWallMover mover = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        Vector3 direction = manualMoveDirection;

        if (autoDirectionTowardsPlayer && PlayerBodySampler.Instance != null)
        {
            direction = PlayerBodySampler.Instance.transform.position - spawnPoint.position;
            if (ignoreHeightDifference) direction.y = 0f;
        }

        mover.Configure(direction, moveSpeed);

        if (resultUI != null)
        {
            MirrorWallDetector detector = mover.GetComponent<MirrorWallDetector>();
            resultUI.SetDetector(detector);
            mover.OnReachedPlayer += _ => resultUI.TriggerFinalCheck();
        }
    }

    /// <summary>
    /// Escolhe qual prefab usar no próximo spawn, de acordo com o modo selecionado.
    /// </summary>
    private MirrorWallMover PickPrefab()
    {
        if (wallPrefabs == null || wallPrefabs.Length == 0)
            return null;

        if (wallPrefabs.Length == 1)
            return wallPrefabs[0];

        if (selectionMode == PrefabSelectionMode.Sequential)
        {
            MirrorWallMover chosen = wallPrefabs[nextSequentialIndex];
            nextSequentialIndex = (nextSequentialIndex + 1) % wallPrefabs.Length;
            return chosen;
        }

        // Random
        int index;
        do
        {
            index = Random.Range(0, wallPrefabs.Length);
        }
        while (avoidImmediateRepeatOnRandom && index == lastRandomIndex);

        lastRandomIndex = index;
        return wallPrefabs[index];
    }
}