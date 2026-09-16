using UnityEngine;

public class WallManager : MonoBehaviour
{
    [Header("Collider da parede")]
    [SerializeField] private Collider targetCollider;

    [Header("Precisão")]
    [Range(3, 12)]
    [SerializeField] private int samplesPerAxis = 6;

    [Header("Intervalo de verificação")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float checkInterval = 0.1f;

    [Header("Erro máximo permitido")]
    [Range(0f, 100f)]
    [SerializeField] private float maxErrorPercentage = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Collider playerCollider;

    private float currentPercentage;
    private float timer;

    private bool playerInside;

    private void Awake()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerCollider = other;
        playerInside = true;

        timer = 0f;

        CalculatePercentage();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!playerInside)
            return;

        if (other != playerCollider)
            return;

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;

            CalculatePercentage();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != playerCollider)
            return;

        playerInside = false;
        playerCollider = null;

        currentPercentage = 0f;
        timer = 0f;
    }

    private void CalculatePercentage()
    {
        if (playerCollider == null)
            return;

        Bounds bounds = playerCollider.bounds;

        int totalPoints = 0;
        int insidePoints = 0;

        int resolution = samplesPerAxis;

        for (int x = 0; x < resolution; x++)
        {
            float fx = x / (float)(resolution - 1);

            for (int y = 0; y < resolution; y++)
            {
                float fy = y / (float)(resolution - 1);

                for (int z = 0; z < resolution; z++)
                {
                    float fz = z / (float)(resolution - 1);

                    Vector3 point = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, fx),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, fy),
                        Mathf.Lerp(bounds.min.z, bounds.max.z, fz)
                    );

                    if (!IsInsideCollider(point, playerCollider))
                        continue;

                    totalPoints++;

                    if (IsInsideCollider(point, targetCollider))
                    {
                        insidePoints++;
                    }
                }
            }
        }

        if (totalPoints == 0)
        {
            currentPercentage = 0f;
            return;
        }

        currentPercentage =
            (insidePoints / (float)totalPoints) * 100f;

        if (debugLogs)
        {
            Debug.Log(
                "Colisão: " +
                currentPercentage.ToString("F1") +
                "%"
            );
        }
    }

    private bool IsInsideCollider(
        Vector3 point,
        Collider collider)
    {
        Vector3 closestPoint =
            collider.ClosestPoint(point);

        return Vector3.SqrMagnitude(
            point - closestPoint
        ) < 0.000001f;
    }

    public void CheckResult()
    {
        if (!playerInside)
        {
            Debug.Log("Jogador não está na parede.");
            return;
        }

        if (currentPercentage <= maxErrorPercentage)
        {
            Debug.Log(
                "PASSOU! " +
                currentPercentage.ToString("F1") +
                "% de erro"
            );
        }
        else
        {
            Debug.Log(
                "FALHOU! " +
                currentPercentage.ToString("F1") +
                "% de erro"
            );
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