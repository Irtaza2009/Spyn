using UnityEngine;

public enum TipType
{
    Attack,
    Defence,
    Stamina
}

public class SpynerTip : MonoBehaviour
{
    public TipType tipType = TipType.Attack;

    [Header("Stats")]
    [Min(0f)] public float movementMultiplier = 1f;
    [Min(0f)] public float spinLossMultiplier = 1f;
    [Min(0f)] public float impactMultiplier = 1f;
    [Min(0f)] public float resistanceMultiplier = 1f;

    private void OnValidate()
    {
        switch (tipType)
        {
            case TipType.Attack:
                movementMultiplier = 1.3f;
                spinLossMultiplier = 1.3f;
                impactMultiplier = 1.5f;
                resistanceMultiplier = 1f;
                break;

            case TipType.Defence:
                movementMultiplier = 0.75f;
                spinLossMultiplier = 1f;
                impactMultiplier = 0.8f;
                resistanceMultiplier = 0.65f;
                break;

            case TipType.Stamina:
                movementMultiplier = 0.9f;
                spinLossMultiplier = 0.65f;
                impactMultiplier = 0.8f;
                resistanceMultiplier = 1.1f;
                break;
        }
    }
}