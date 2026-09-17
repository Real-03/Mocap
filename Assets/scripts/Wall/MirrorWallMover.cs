using UnityEngine;
using System;

/// <summary>
/// Move a parede/obstáculo a que está anexado em linha reta, a velocidade
/// constante, e avisa quando "cruza" a posição do jogador (momento certo para
/// avaliar o desafio) e quando já passou o suficiente para poder ser destruída.
///
/// Não faz o próprio jogador mover-se nem afeta a física dele — só desloca o
/// GameObject da parede. A deteção de colisão continua a ser feita inteiramente
/// pelo MirrorWallDetector, que já está preparado para paredes em movimento
/// (recalcula tudo a cada checkInterval a partir da posição atual da parede).
/// </summary>
[RequireComponent(typeof(MirrorWallDetector))]
public class MirrorWallMover : MonoBehaviour
{
    [Tooltip("Direção de movimento em World Space. Normalizada automaticamente. Pode ser sobrescrita em runtime via Configure().")]
    [SerializeField] private Vector3 moveDirection = Vector3.back;

    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Depois de a parede cruzar o jogador, quanto tempo (em distância percorrida) espera antes de se autodestruir.")]
    [SerializeField] private float destroyDistanceAfterPlayer = 3f;

    /// <summary>Disparado UMA vez, no instante em que a parede cruza a posição do jogador — o momento certo para chamar CheckResult()/TriggerFinalCheck().</summary>
    public event Action<MirrorWallDetector> OnReachedPlayer;

    /// <summary>Disparado UMA vez, mesmo antes de a parede se autodestruir depois de já ter passado o jogador.</summary>
    public event Action OnFinishedPassing;

    private MirrorWallDetector detector;
    private Transform playerTransform;
    private bool judged;

    private void Awake()
    {
        detector = GetComponent<MirrorWallDetector>();
        moveDirection = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector3.back;
    }

    private void Start()
    {
        playerTransform = PlayerBodySampler.Instance != null ? PlayerBodySampler.Instance.transform : null;

        if (playerTransform == null)
        {
            Debug.LogWarning("[MirrorWallMover] Nenhum PlayerBodySampler.Instance encontrado — a parede move-se, mas OnReachedPlayer nunca vai disparar.", this);
        }
    }

    /// <summary>
    /// Define direção e velocidade a partir de código, tipicamente chamado pelo
    /// spawner logo depois de instanciar a parede.
    /// </summary>
    public void Configure(Vector3 newDirection, float newSpeed)
    {
        if (newDirection.sqrMagnitude > 0.0001f)
        {
            moveDirection = newDirection.normalized;
        }
        moveSpeed = newSpeed;
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (playerTransform == null) return;

        // Distância assinada do jogador em relação à parede, medida ao longo do eixo
        // de movimento: positiva enquanto o jogador ainda está "à frente" da parede
        // (no sentido do movimento), negativa depois de a parede o ultrapassar.
        float signedDistanceToPlayer = Vector3.Dot(playerTransform.position - transform.position, moveDirection);

        if (!judged && signedDistanceToPlayer <= 0f)
        {
            judged = true;
            OnReachedPlayer?.Invoke(detector);
        }

        if (judged && -signedDistanceToPlayer >= destroyDistanceAfterPlayer)
        {
            OnFinishedPassing?.Invoke();
            Destroy(gameObject);
        }
    }
}
