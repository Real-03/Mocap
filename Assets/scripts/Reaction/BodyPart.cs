using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public enum BodyPartType
    {
        RightHand,
        LeftHand,
        RightFoot,
        LeftFoot
    }

    [Header("Parte do corpo")]
    public BodyPartType bodyPart;
}