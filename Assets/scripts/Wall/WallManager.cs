using UnityEngine;

public class WallManager : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Collider targetCollider;

    [Header("Erro permitido")]
    [Range(0f, 100f)]
    [SerializeField] private float maxErrorPercentage = 10f;

    [Header("Precisão")]
    [Range(5, 30)]
    [SerializeField] private int samplesPerAxis = 15;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Collider playerCollider;

    private float currentPercentage;
    private bool playerInside;

    private void Awake()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerCollider = other;
        playerInside = true;

        currentPercentage = CalculateIntersection();

        if (showDebug)
        {
            Debug.Log(
                "Corpo dentro da forma: " +
                currentPercentage.ToString("F1") +
                "%"
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerCollider)
        {
            playerInside = false;
            playerCollider = null;
            currentPercentage = 0f;
        }
    }

    private float CalculateIntersection()
    {
        if (playerCollider == null)
            return 0f;

        Bounds bounds = playerCollider.bounds;

        int totalSamples = 0;
        int insideSamples = 0;

        for (int x = 0; x < samplesPerAxis; x++)
        {
            for (int y = 0; y < samplesPerAxis; y++)
            {
                for (int z = 0; z < samplesPerAxis; z++)
                {
                    float fx = x / (float)(samplesPerAxis - 1);
                    float fy = y / (float)(samplesPerAxis - 1);
                    float fz = z / (float)(samplesPerAxis - 1);

                    Vector3 point = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, fx),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, fy),
                        Mathf.Lerp(bounds.min.z, bounds.max.z, fz)
                    );

                    // Verifica se o ponto pertence ao MeshCollider
                    if (!IsPointInsideCollider(
                        point,
                        playerCollider))
                    {
                        continue;
                    }

                    totalSamples++;

                    // Verifica se também está dentro da forma
                    if (IsPointInsideCollider(
                        point,
                        targetCollider))
                    {
                        insideSamples++;
                    }
                }
            }
        }

        if (totalSamples == 0)
            return 0f;

        return (insideSamples / (float)totalSamples) * 100f;
    }

    private bool IsPointInsideCollider(
        Vector3 point,
        Collider collider)
    {
        Vector3 closest =
            collider.ClosestPoint(point);

        float distance =
            Vector3.Distance(point, closest);

        return distance < 0.001f;
    }

    public void CheckResult()
    {
        if (!playerInside)
        {
            Debug.Log("Jogador não está na parede.");
            return;
        }

        Debug.Log(
            "========================"
        );

        Debug.Log(
            "COLISÃO: " +
            currentPercentage.ToString("F1") +
            "%"
        );

        Debug.Log(
            "LIMITE: " +
            maxErrorPercentage +
            "%"
        );

        if (currentPercentage <= maxErrorPercentage)
        {
            Debug.Log("PASSOU! ✅");
        }
        else
        {
            Debug.Log("FALHOU! ❌");
        }
    }

    public float GetCurrentPercentage()
    {
        return currentPercentage;
    }

    public bool HasPassed()
    {
        return currentPercentage <= maxErrorPercentage;
    }
}