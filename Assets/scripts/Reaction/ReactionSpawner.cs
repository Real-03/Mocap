using UnityEngine;

public class ReactionSpawner : MonoBehaviour
{
    [Header("Objetos que podem aparecer")]
    public GameObject[] targetPrefabs;

    [Header("Área de Spawn")]
    public BoxCollider spawnArea;

    [Header("Profundidade fixa")]
    public float fixedZ;

    [Header("Tempo entre objetos")]
    public float timeBetweenTargets = 0.5f;

    private GameObject currentTarget;

    private void Start()
    {
        // Se não definires manualmente, usa a profundidade
        // do próprio SpawnArea
        if (spawnArea != null)
            fixedZ = spawnArea.transform.position.z;

        Invoke(nameof(SpawnTarget), 1f);
    }

    public void SpawnTarget()
    {
        if (currentTarget != null)
            return;

        if (targetPrefabs.Length == 0)
        {
            Debug.LogWarning("Não existem Target Prefabs!");
            return;
        }

        if (spawnArea == null)
        {
            Debug.LogWarning("Não foi definida uma Spawn Area!");
            return;
        }

        // Escolher prefab aleatório
        GameObject prefab =
            targetPrefabs[Random.Range(0, targetPrefabs.Length)];

        // Obter limites da área
        Bounds bounds = spawnArea.bounds;

        // X e Y aleatórios
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        // Z sempre igual
        Vector3 spawnPosition = new Vector3(
            randomX,
            randomY,
            fixedZ
        );

        // Rotação de -90º no eixo X
        Quaternion spawnRotation = Quaternion.Euler(-90f, 0f, 0f);

        currentTarget = Instantiate(
            prefab,
            spawnPosition,
            spawnRotation
        );

        StartCoroutine(WaitForTarget());
    }

    private System.Collections.IEnumerator WaitForTarget()
    {
        while (currentTarget != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(timeBetweenTargets);

        SpawnTarget();
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
            return;

        Gizmos.color = Color.green;

        Bounds bounds = spawnArea.bounds;

        // Representa apenas o plano X/Y
        Vector3 center = new Vector3(
            bounds.center.x,
            bounds.center.y,
            fixedZ
        );

        Vector3 size = new Vector3(
            bounds.size.x,
            bounds.size.y,
            0.05f
        );

        Gizmos.DrawWireCube(center, size);
    }
}