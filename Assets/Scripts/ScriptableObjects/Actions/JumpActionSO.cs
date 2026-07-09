using UnityEngine;

[CreateAssetMenu(fileName = "Jump Action", menuName = "Player/Actions/Jump Action")]
public class JumpActionSO: BaseActionSO
{
    public float MaxJumpHeight = 5f;
    public float MaxJumpLateralDistance = 10f;
    public float FastFallMultiplier = 3f;

    [Range(0f, 1f)]
    public float jumpCutoffMultiplier = 0.25f; // Multiplier for vertical velocity when jump is cut short

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(ActionName))
            ActionName = "Jump";

        if (string.IsNullOrEmpty(ActionDescription))
            ActionDescription = "Basic jump action with parameterized controls";
    }
}
