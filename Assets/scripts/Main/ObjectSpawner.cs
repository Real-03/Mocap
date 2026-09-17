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
    // Ammount of time between the first and second spawn
    // The actual time gets quicker over time
    private int timerMultiplier;

    // Total amount of objects that can spawn
    private int totalCount => object1Count + object2Count + bombCount;
    private int currentCount => currentobj1 + currentobj2 + currentbomb;

    private IEnumerator Ready()
    {
        DisableButton();
        for (int i = 0; i < totalCount; i++)
        {
            Spawn();
            yield return 
                new WaitForSeconds(
                    timerMultiplier
                    // Shorten the time for each spawn
                    / i+1);
        }
    }

    private void Spawn()
    {
        int rand = Random.Range(0, totalCount);
        if (rand)
    }

    private void DisableButton()
    {

    }
}
