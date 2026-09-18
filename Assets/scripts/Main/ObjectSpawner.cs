using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private int object1Count;
    private int currentobj1;
    [SerializeField]
    private int object2Count;
    private int currentobj2;
    [SerializeField]
    private int bombCount;
    private int currentbomb;

    [SerializeField]
    private float spawnRadius;
    [SerializeField]
    private float spawnHeight;

    [SerializeField]
    // Ammount of time between the first and second spawn
    // The actual time gets quicker over time
    private int timerMultiplier;

    // Total amount of objects that can spawn
    private int totalCount => object1Count + object2Count + bombCount;
    private int currentCount => currentobj1 + currentobj2 + currentbomb;

    private IEnumerator Ready()
    {
        DisableButton();

        // Start spawning objects
        for (int i = 0; i < totalCount; i++)
        {
            Spawn();
            yield return 
                new WaitForSeconds(
                    timerMultiplier
                    // Shorten the time for each spawn
                    / i+1);
        }
        // Finish and enable next sequence
    }

    private void Spawn()
    {
        if (currentCount <= 0)
            return;

        int rand = Random.Range(0, totalCount);
        if (rand - currentobj1 <= 0)
        {
            // Spawn obj1
        }
        else if (rand - currentobj1 - currentobj2 <= 0)
        {
            // Spawn obj 2
        }
        else
        {
            // Spawn bomb
        }
    }

    private float GetAngle()
    {
        // Get the spawn angle
        return Random.Range(0, 2 * Mathf.PI);
    }

    private Vector3 GetCoordinates(float angle)
    {
        // Get 2 points that are part of the circle
        float posX = spawnRadius * Mathf.Cos(angle);
        float posY = spawnRadius * Mathf.Sin(angle);
        return new Vector3(
            posX, 
            posY, 
            spawnHeight);
    }

    private void DisableButton()
    {
        // Code to remove button
    }
}
