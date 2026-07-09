using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Grapple")]
public class GrappleAbilitySO : AbilitySO
{
    public float MaxGrappleDistance = 10f;
    public float DelayTime = 1f;
    public float Cooldown = 2f;
    public float OvershootHeight = 1f;
    public float OffsetHeight = 1f;
    public float OffsetDistance = 1f;
    public float MaxCheckAngle = 45f;
}
