using UnityEngine;
using System;

/// <summary>
/// Deteta que percentagem do corpo do jogador (representado por pontos amostrados
/// pelo PlayerBodySampler) está sobre a parte SÓLIDA de um obstáculo, em vez de
/// sobre a abertura/buraco.
///
/// Técnica: para cada ponto do corpo, dispara-se UM raycast ao longo do eixo de
/// profundidade local (definido pelo "Wall Collider" de referência), na mesma
/// coluna (x,y) do ponto. Se o raio acertar em QUALQUER collider dentro da Wall
/// Layer Mask — Mesh Collider, Box Collider, Capsule Collider, etc. — esse ponto
/// conta como bloqueado. Já não é preciso ser exatamente o Collider atribuído: a
/// deteção passa a considerar todos os colliders que estejam à frente na layer
/// certa, por isso funciona com obstáculos compostos por vários tipos de collider
/// em simultâneo.
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
    [Tooltip("Collider usado APENAS como referência de orientação/profundidade do raio (a origem, a direção 'forward' e a espessura estimada vêm daqui). Se vazio, usa o Collider deste GameObject. A deteção em si aceita QUALQUER collider dentro da Wall Layer Mask, não só este.")]
    [SerializeField] private Collider wallCollider;
    [Tooltip("Layer Mask com TODAS as layers que contêm obstáculos à frente do corpo — pode incluir vários objetos e tipos de collider diferentes (Mesh, Box, Capsule, etc.), todos contam como bloqueio.")]
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

    /// <summary>Última percentagem calculada do corpo sobre zona sólida de algum obstáculo.</summary>
    public float CurrentCollidingPercentage { get; private set; }

    /// <summary>Resultado da última avaliação: true = dentro da margem de erro (passou).</summary>
    public bool LastCheckPassed { get; private set; } = true;

    /// <summary>Disparado sempre que a percentagem é recalculada (a cada checkInterval).</summary>
    public event Action<bool, float> OnEvaluated;

    private float timer;
    private bool[] lastPointBlocked;
    private Vector3[] lastWorldPoints;
    private float wallHalfDepth;
    private Vector3 localDepthAxis;
    private float depthAxisCenterLocal;

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

        // Deteta automaticamente qual o eixo local mais "fino" da mesh — esse é quase
        // sempre o eixo de espessura de um painel/parede achatada, independentemente
        // da orientação com que a mesh foi importada (Blender, Maya, etc. usam
        // convenções de eixos diferentes do Unity, e isto evita teres de configurar
        // isso à mão por cada mesh nova).
        ComputeDepthAxisAndThickness();

        // Reset de estado, útil se este componente for reativado num objeto reaproveitado.
        timer = 0f;
        lastPointBlocked = null;
        lastWorldPoints = null;
        CurrentCollidingPercentage = 0f;
        LastCheckPassed = true;
    }

    /// <summary>
    /// Mede a mesh do wallCollider (se for um MeshCollider) em espaço LOCAL — não
    /// afetado pela rotação do transform — e usa o eixo com menor extensão como
    /// direção do raio. Isto é o que faz o sistema funcionar com qualquer mesh
    /// importada, e não só com um cubo perfeito (que por acaso tem a mesma
    /// espessura em todos os eixos, escondendo o problema).
    /// </summary>
    private void ComputeDepthAxisAndThickness()
    {
        Vector3 localExtents;
        Vector3 localCenter;

        if (wallCollider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            // bounds da mesh em espaço LOCAL DA MESH — não depende da posição do pivot
            // do objeto, por isso funciona mesmo que o artista tenha colocado o pivot
            // na base, num canto, ou em qualquer sítio que não seja o centro.
            Bounds meshBounds = meshCollider.sharedMesh.bounds;
            localExtents = Vector3.Scale(meshBounds.extents, wallCollider.transform.lossyScale);
            localCenter = Vector3.Scale(meshBounds.center, wallCollider.transform.lossyScale);
        }
        else
        {
            // Fallback para Box/Capsule/etc.: aproxima a partir do bounds mundial,
            // convertendo o centro para espaço local do transform.
            Bounds worldBounds = wallCollider.bounds;
            localExtents = worldBounds.extents;
            localCenter = wallCollider.transform.InverseTransformPoint(worldBounds.center);
        }

        localExtents = new Vector3(Mathf.Abs(localExtents.x), Mathf.Abs(localExtents.y), Mathf.Abs(localExtents.z));

        if (localExtents.x <= localExtents.y && localExtents.x <= localExtents.z)
        {
            localDepthAxis = Vector3.right;
            wallHalfDepth = Mathf.Max(0.05f, localExtents.x);
            depthAxisCenterLocal = localCenter.x;
        }
        else if (localExtents.y <= localExtents.x && localExtents.y <= localExtents.z)
        {
            localDepthAxis = Vector3.up;
            wallHalfDepth = Mathf.Max(0.05f, localExtents.y);
            depthAxisCenterLocal = localCenter.y;
        }
        else
        {
            localDepthAxis = Vector3.forward;
            wallHalfDepth = Mathf.Max(0.05f, localExtents.z);
            depthAxisCenterLocal = localCenter.z;
        }
    }

    private void Awake()
    {
        AutoConfigure();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        if (playerSampler == null) return;

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
    /// Usa o Collider de referência apenas para saber onde está a zona de interesse.
    /// NÃO é usado para calcular a percentagem final.
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
        Vector3 rayDirWorld = wallT.TransformDirection(localDepthAxis).normalized;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 local = wallT.InverseTransformPoint(points[i]);

            // Zera a coordenada ao longo do eixo de profundidade e recua bem antes do
            // CENTRO REAL da mesh nesse eixo (não da origem do objeto — importante para
            // meshes importadas com o pivot descentrado), mantendo as outras duas
            // coordenadas (a "coluna" do ponto do corpo) exatamente onde estavam.
            Vector3 rayOriginLocal = local - Vector3.Scale(local, localDepthAxis)
                + localDepthAxis * (depthAxisCenterLocal - wallHalfDepth * 3f);
            Vector3 rayOriginWorld = wallT.TransformPoint(rayOriginLocal);

            // Aceita QUALQUER collider dentro da Wall Layer Mask — Mesh Collider, Box
            // Collider, Capsule Collider, etc. Já não exige que seja exatamente o
            // wallCollider atribuído, por isso funciona com obstáculos compostos por
            // vários colliders de tipos diferentes em simultâneo.
            bool blocked = Physics.Raycast(
                rayOriginWorld,
                rayDirWorld,
                out RaycastHit hit,
                rayLength,
                wallLayerMask,
                QueryTriggerInteraction.Collide
            );

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