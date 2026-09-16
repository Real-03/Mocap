using UnityEngine;
using TMPro;

public class ReactionTarget : MonoBehaviour
{
    public BodyPart.BodyPartType requiredBodyPart;

    public float maxTime = 1f;

    public TextMeshProUGUI resultText;

    private float spawnTime;
    private bool alreadyTouched = false;

    private void Start()
    {
        spawnTime = Time.time;
        Invoke(nameof(TimeOut), maxTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTouched)
            return;

        BodyPart bodyPart = other.GetComponent<BodyPart>();

        if (bodyPart == null)
            return;

        if (bodyPart.bodyPart == requiredBodyPart)
        {
            CorrectTouch(bodyPart);
        }
        else
        {
            WrongTouch(bodyPart);
        }
    }

    private void CorrectTouch(BodyPart bodyPart)
    {
        alreadyTouched = true;

        CancelInvoke(nameof(TimeOut));

        float reactionTime = Time.time - spawnTime;

        Debug.Log(
            "CORRETO! " +
            bodyPart.bodyPart +
            " | Tempo: " +
            reactionTime.ToString("F3") +
            "s"
        );

        if (resultText != null)
        {
            resultText.text =
                "CORRETO!\n" +
                "Tempo: " +
                reactionTime.ToString("F3") +
                " s";
        }

        Destroy(gameObject);
    }

    private void WrongTouch(BodyPart bodyPart)
    {
        Debug.Log(
            "ERRADO! Usaste: " +
            bodyPart.bodyPart +
            " | Necessário: " +
            requiredBodyPart
        );

        if (resultText != null)
        {
            resultText.text =
                "ERRADO!\n" +
                "Usaste: " +
                bodyPart.bodyPart +
                "\nPrecisavas: " +
                requiredBodyPart;
        }
    }

    private void TimeOut()
    {
        if (alreadyTouched)
            return;

        Debug.Log(
            "TEMPO ESGOTADO! Necessário: " +
            requiredBodyPart
        );

        if (resultText != null)
        {
            resultText.text = "TEMPO ESGOTADO!";
        }

        Destroy(gameObject);
    }
}