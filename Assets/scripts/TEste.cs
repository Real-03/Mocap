using UnityEngine;

public class TEste : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered: " + other.gameObject.name);
    }
}
