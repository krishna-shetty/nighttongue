using UnityEngine;

[CreateAssetMenu(fileName = "TongueLimitSO", menuName = "Abilities/TongueLimit")]
public class TongueLimitSO : ScriptableObject
{
    public float MaxAngle = 180f;
    public float MinAngle = 0f;

    [Header("Near-Body Block")]
    [Min(0f)] public float InnerRadius = 0.45f;   // ÌùÉí½ûÈë°ë¾¶
}
