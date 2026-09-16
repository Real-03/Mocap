using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Amostra um subconjunto fixo de vértices da mesh do jogador (skinned ou estática)
/// e devolve as suas posições em World Space, atualizadas sob pedido.
///
/// Não usa Collider.ClosestPoint nem Bounds para representar o corpo — usa os
/// vértices reais da mesh, por isso respeita a geometria verdadeira do corpo.
/// </summary>
public class PlayerBodySampler : MonoBehaviour
{
    [Header("Fonte da mesh (escolher UMA)")]
    [Tooltip("Usar isto se o corpo for uma Skinned Mesh (motion capture).")]
    [SerializeField] private SkinnedMeshRenderer skinnedRenderer;

    [Tooltip("Usar isto em alternativa se o corpo for uma mesh estática.")]
    [SerializeField] private MeshFilter staticMeshFilter;

    [Header("Amostragem")]
    [Tooltip("Número de pontos usados para representar o corpo. 150-300 costuma ser suficiente.")]
    [SerializeField] private int targetSampleCount = 250;

    private int[] sampleIndices;
    private Mesh bakedMesh;
    private readonly List<Vector3> vertexBuffer = new List<Vector3>();
    private Vector3[] worldSamplePoints;
    private Transform sourceTransform;
    private bool useSkinned;

    /// <summary>Últimas posições em World Space calculadas por RefreshWorldPoints().</summary>
    public Vector3[] WorldSamplePoints => worldSamplePoints;
    public int SampleCount => sampleIndices?.Length ?? 0;

    private void Awake()
    {
        useSkinned = skinnedRenderer != null;
        Mesh sourceMesh = useSkinned ? skinnedRenderer.sharedMesh : staticMeshFilter.sharedMesh;
        sourceTransform = useSkinned ? skinnedRenderer.transform : staticMeshFilter.transform;

        if (sourceMesh == null)
        {
            Debug.LogError("[PlayerBodySampler] Nenhuma mesh atribuída (skinnedRenderer ou staticMeshFilter).");
            enabled = false;
            return;
        }

        BuildSampleIndices(sourceMesh.vertexCount);
        worldSamplePoints = new Vector3[sampleIndices.Length];

        if (useSkinned)
        {
            bakedMesh = new Mesh();
        }

        // Primeira atualização já no arranque, para que WorldSamplePoints não venha vazio.
        RefreshWorldPoints();
    }

    private void BuildSampleIndices(int vertexCount)
    {
        int count = Mathf.Clamp(targetSampleCount, 1, vertexCount);
        sampleIndices = new int[count];

        // Amostragem por passo fixo (stride), não aleatória — cobre a mesh de forma
        // uniforme em vez de enviesar para uma zona do corpo.
        float step = (float)vertexCount / count;
        for (int i = 0; i < count; i++)
        {
            sampleIndices[i] = Mathf.Min(vertexCount - 1, Mathf.FloorToInt(i * step));
        }
    }

    /// <summary>
    /// Recalcula e devolve as posições em World Space dos pontos de amostra.
    /// Chamar apenas no intervalo configurado no detector (ex.: a cada 0.1s), nunca por frame.
    /// </summary>
    public Vector3[] RefreshWorldPoints()
    {
        Matrix4x4 localToWorld = sourceTransform.localToWorldMatrix;

        if (useSkinned)
        {
            // BakeMesh dá-nos a pose atual (deformada pelo mocap) em espaço local do renderer.
            skinnedRenderer.BakeMesh(bakedMesh, true);
            bakedMesh.GetVertices(vertexBuffer);
        }
        else
        {
            staticMeshFilter.sharedMesh.GetVertices(vertexBuffer);
        }

        for (int i = 0; i < sampleIndices.Length; i++)
        {
            worldSamplePoints[i] = localToWorld.MultiplyPoint3x4(vertexBuffer[sampleIndices[i]]);
        }

        return worldSamplePoints;
    }
}