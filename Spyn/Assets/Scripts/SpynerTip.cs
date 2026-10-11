
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
                movementMultiplier = 1.25f;
                spinLossMultiplier = 1.30f;
                impactMultiplier = 1.45f;
                resistanceMultiplier = 0.90f;
                break;

            case TipType.Defence:
                movementMultiplier = 0.85f;
                spinLossMultiplier = 0.90f;
                impactMultiplier = 0.85f;
                resistanceMultiplier = 1.40f;
                break;

            case TipType.Stamina:
                movementMultiplier = 0.95f;
                spinLossMultiplier = 0.65f;
                impactMultiplier = 0.85f;
                resistanceMultiplier = 1.05f;
                break;
        }
    }
}