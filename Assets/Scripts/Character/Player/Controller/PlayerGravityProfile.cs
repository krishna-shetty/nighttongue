using UnityEngine;

public class PlayerGravityProfile
{
    // Should be treated like constants
    private float _jumpGravity;
    private float _fastFallGravity;
    private float _constantGravityLateralDistance;
    private float _initialJumpSpeed;

    public static PlayerGravityProfile Instance { get; private set; }

    // Should only be called once in PlayerController.Awake(), returns the active instance
    public static PlayerGravityProfile Create(MoveActionSO move, JumpActionSO jump)
    {
        if (Instance != null)
        {
            return Instance; // Ensure initialization happens only once
        }
        Instance = new PlayerGravityProfile();
        Instance.SetInitialConstants(move, jump);
        return Instance;
    }

    private void SetInitialConstants(MoveActionSO move, JumpActionSO jump)
    {
        _constantGravityLateralDistance = CalculateGravityLateralDistance(move, jump);
        _jumpGravity = CalculateJumpGravity(move, jump);
        _fastFallGravity = CalculateFastFallGravity(move, jump);
        _initialJumpSpeed = CalculateInitialJumpSpeed(move, jump);
    }

    public float CalculateGravityLateralDistance(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = (2 * jump.MaxJumpLateralDistance * Mathf.Sqrt(jump.FastFallMultiplier)) / (1 + Mathf.Sqrt(jump.FastFallMultiplier));
        return constantGravityLateralDistance;
    }

    public float CalculateJumpGravity(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = CalculateGravityLateralDistance(move, jump);
        float jumpGravity = (-2f * jump.MaxJumpHeight * Mathf.Pow(move.MaxHorizontalSpeed, 2.0f)) / (Mathf.Pow((constantGravityLateralDistance / 2.0f), 2.0f));
        return jumpGravity;
    }

    public float CalculateFastFallGravity(MoveActionSO move, JumpActionSO jump)
    {
        float jumpGravity = CalculateJumpGravity(move, jump);
        float fastFallGravity = jumpGravity * jump.FastFallMultiplier;
        return fastFallGravity;
    }

    public float CalculateInitialJumpSpeed(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = CalculateGravityLateralDistance(move, jump);
        float initialJumpSpeed = (2.0f * jump.MaxJumpHeight * move.MaxHorizontalSpeed) / (constantGravityLateralDistance / 2.0f);
        return initialJumpSpeed;
    }

    public float GetJumpGravity() { return _jumpGravity; }

    public float GetFastFallGravity() { return _fastFallGravity; }

    public float GetConstantGravityLateralDistance() { return _constantGravityLateralDistance; }

    public float GetInitialJumpSpeed() { return _initialJumpSpeed; }
}
