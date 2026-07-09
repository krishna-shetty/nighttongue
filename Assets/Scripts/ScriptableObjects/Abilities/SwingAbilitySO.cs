using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Swing")]
public class SwingAbilitySO : AbilitySO
{
    public float MinRappleDistance = 1f;
    public float MaxRappleDistance = 10f;
    public float InitialTongueLength = 5f;
    public float TongueLerpTime = 0.1f;
    public float DelayTime = 1f;
    public float Cooldown = 2f;
    public float MaxSwingAngle = 45f;
    public float SpringConstant = 0.5f;
    public float Damping = 0.1f;
    public float UserControlForce = 5f;
    public float RappelUpSpeed = 1f;
    public float RappelDownSpeed = 1f;
    public float MaxInitialSwingSpeed = 10f;
}
