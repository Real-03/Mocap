using BeatStyleGame;
using TMPro;
using UnityEngine;

public class SubwayContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Got Here");
        BodyPart bodyPart = other.GetComponent<BodyPart>();

        if (bodyPart == null)
            return;

        FindAnyObjectByType<SpawnManager>().gameObject.SetActive(false);
        FindAnyObjectByType<TextMeshProUGUI>().gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
