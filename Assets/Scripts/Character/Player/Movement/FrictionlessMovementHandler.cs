using UnityEngine;

public class FrictionlessMovementHandler : MovementHandlerBase
{
    public FrictionlessMovementHandler(PlayerController controller) : base(controller)
    {
    }

    public override void Apply(float gravity, MoveActionSO move)
    {
        ApplyExternalForces();
        ApplyVerticalMovement(gravity);

        if (Context.UsingPrecisionMovement())
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
        SimulateStep();
    }
}
