using UnityEngine;
using System;

/// <summary>
/// Deteta que percentagem do corpo do jogador (representado por pontos amostrados
/// pelo PlayerBodySampler) está sobre a parte SÓLIDA da parede, em vez de sobre a
/// abertura/buraco.
///
/// Técnica: para cada ponto do corpo, dispara-se UM raycast ao longo do eixo de
/// profundidade local da parede, na mesma coluna (x,y) do ponto. Se o raio acertar
/// na geometria real da parede, esse ponto está sobre uma zona sólida (bloqueado).
/// Caso contrário, está sobre o buraco (livre).
///
/// Isto funciona com Mesh Colliders NÃO-convexos porque Physics.Raycast, ao
/// contrário de ClosestPoint/ComputePenetration, não exige convexidade. E é barato
/// porque só corre a um intervalo configurável, não por frame, e só quando há
/// overlap grosseiro (broad-phase por AABB).
/// </summary>
[RequireComponent(typeof(Collider))]
public class MirrorWallDetector : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerBodySampler playerSampler;
    [Tooltip("O Mesh Collider (não-convexo) da parede. Se vazio, usa o Collider neste GameObject.")]
    [SerializeField] private Collider wallCollider;
    [Tooltip("Layer Mask contendo APENAS a layer da parede.")]
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Regras do desafio")]
    [Range(0f, 100f)]
    [SerializeField] private float maxErrorPercentage = 10f;
    [Tooltip("Intervalo, em segundos, entre cada recálculo da percentagem.")]
    [SerializeField] private float checkInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [Tooltip("Só regista no Console quando CheckResult() é chamado explicitamente — nunca por frame.")]
    [SerializeField] private bool logOnCheckResult = false;

    /// <summary>Última percentagem calculada do corpo sobre zona sólida da parede.</summary>
    public float CurrentCollidingPercentage { get; private set; }

    /// <summary>Resultado da última avaliação: true = dentro da margem de erro (passou).</summary>
    public bool LastCheckPassed { get; private set; } = true;

    /// <summary>Disparado sempre que a percentagem é recalculada (a cada checkInterval).</summary>
    public event Action<bool, float> OnEvaluated;

    private float timer;
    private bool[] lastPointBlocked;
    private Vector3[] lastWorldPoints;
    private float wallHalfDepth;
     public void AutoConfigure()
    {
        if (wallCollider == null) wallCollider = GetComponent<Collider>();
        if (playerSampler == null) playerSampler = PlayerBodySampler.Instance;

        if (playerSampler == null)
        {
            Debug.LogError("[MirrorWallDetector] Não foi encontrado nenhum PlayerBodySampler.Instance. Confirma que o jogador já existe e está ativo antes desta parede ser criada.", this);
            enabled = false;
            return;
        }

        if (wallLayerMask.value == 0)
        {
            wallLayerMask = 1 << gameObject.layer;
        }


        // Reset de estado, útil se este componente for reativado num objeto reaproveitado.
        timer = 0f;
        lastPointBlocked = null;
        lastWorldPoints = null;
        CurrentCollidingPercentage = 0f;
        LastCheckPassed = true;
    }
    private void Awake()
    {
        AutoConfigure();
        if (wallCollider == null) wallCollider = GetComponent<Collider>();

        // Estima a "profundidade" a percorrer pelo raio a partir do próprio bounds da parede.
        // Não é usado para calcular a percentagem — só para dimensionar o raio.
        wallHalfDepth = Mathf.Max(0.1f, wallCollider.bounds.extents.magnitude * 0.5f);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        if (!BroadPhaseOverlap())
        {
            CurrentCollidingPercentage = 0f;
            LastCheckPassed = true;
            return;
        }

        EvaluatePercentage();
    }

    /// <summary>
    /// Teste de AABB muito barato, só para decidir se vale a pena fazer os raycasts.
    /// NÃO é usado para calcular a percentagem final — só liga/desliga o cálculo real.
    /// </summary>
    private bool BroadPhaseOverlap()
    {
        Vector3[] points = playerSampler.WorldSamplePoints;
        if (points == null || points.Length == 0) return false;

        Bounds playerBounds = new Bounds(points[0], Vector3.zero);
        for (int i = 1; i < points.Length; i++)
        {
            playerBounds.Encapsulate(points[i]);
        }

        Bounds wallBounds = wallCollider.bounds;
        wallBounds.Expand(0.3f); // margem de segurança

        return wallBounds.Intersects(playerBounds);
    }

    private void EvaluatePercentage()
    {
        Vector3[] points = playerSampler.RefreshWorldPoints();
        lastWorldPoints = points;

        if (lastPointBlocked == null || lastPointBlocked.Length != points.Length)
            lastPointBlocked = new bool[points.Length];

        Transform wallT = wallCollider.transform;
        int blockedCount = 0;
        float rayLength = wallHalfDepth * 6f;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 local = wallT.InverseTransformPoint(points[i]);

            // Origem do raio bem antes da parede, na mesma coluna (x,y) do ponto do corpo,
            // disparado ao longo do eixo de profundidade local (forward) da parede.
            Vector3 rayOriginLocal = new Vector3(local.x, local.y, -wallHalfDepth * 3f);
            Vector3 rayOriginWorld = wallT.TransformPoint(rayOriginLocal);
            Vector3 rayDirWorld = wallT.forward;

            bool blocked = Physics.Raycast(
                rayOriginWorld,
                rayDirWorld,
                out RaycastHit hit,
                rayLength,
                wallLayerMask,
                QueryTriggerInteraction.Collide
            ) && hit.collider == wallCollider;

            lastPointBlocked[i] = blocked;
            if (blocked) blockedCount++;
        }

        CurrentCollidingPercentage = points.Length > 0 ? (blockedCount / (float)points.Length) * 100f : 0f;
        LastCheckPassed = CurrentCollidingPercentage <= maxErrorPercentage;
        OnEvaluated?.Invoke(LastCheckPassed, CurrentCollidingPercentage);
    }

    /// <summary>
    /// Chamar isto (ex.: quando o jogador tenta atravessar, ou no fim do desafio)
    /// para obter o resultado atual. NÃO recalcula nada — usa o último valor já
    /// calculado no intervalo configurado (checkInterval), por isso é barato de chamar
    /// a qualquer momento.
    /// </summary>
    public bool CheckResult()
    {
        if (logOnCheckResult)
        {
            Debug.Log($"[MirrorWallDetector] {CurrentCollidingPercentage:F1}% em colisão (limite {maxErrorPercentage}%) -> {(LastCheckPassed ? "PASSOU" : "FALHOU")}");
        }
        return LastCheckPassed;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || lastWorldPoints == null) return;

        for (int i = 0; i < lastWorldPoints.Length; i++)
        {
            bool blocked = lastPointBlocked != null && i < lastPointBlocked.Length && lastPointBlocked[i];
            Gizmos.color = blocked ? Color.red : Color.green;
            Gizmos.DrawSphere(lastWorldPoints[i], 0.02f);
        }
    }
}