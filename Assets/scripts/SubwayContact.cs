using BeatStyleGame;
using TMPro;
using UnityEngine;
using UnityVicon;

public class SubwayContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<SubjectScript>() == null)
            return;
        Debug.Log("Got Here");

        FindAnyObjectByType<SpawnManager>().gameObject.SetActive(false);
        FindAnyObjectByType<TextMeshProUGUI>().fontSize = 188;
        Destroy(gameObject);
    }
}
