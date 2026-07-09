using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Tongue Transform")]
public class TongueTransformSO : AbilitySO
{
    public MoveActionSO moveAction;
    public JumpActionSO jumpAction;
    public float SlopeForceMultiplier = 2f;
    public float MinSlopeAngle = 5f;
    public float Cooldown = 1f;
}

