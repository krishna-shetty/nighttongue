using Unity.Mathematics;
using UnityEngine;

public class AirMovementHandler : MovementHandlerBase
{
    public AirMovementHandler(PlayerController controller) : base(controller)
    {
        if (Context && Context.StateMultiplier) stateMultiplier = Context.StateMultiplier.Airborne;
    }

    public override void Apply(float gravity, MoveActionSO move)
    {
        if (Context == null)
        {
            Debug.LogWarning("[AirMovementHandler] Context is null; skipping Apply.");
            return;
        }

        move ??= Context.MoveAction;
        if (move == null)
        {
            Debug.LogWarning("[AirMovementHandler] MoveAction is null; skipping Apply.");
            return;
        }

        ApplyExternalForces(stateMultiplier);
        ApplyVerticalMovement(gravity);

        if (Context.UsingPrecisionMovement())
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
            ApplyFriction(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
        SimulateStep();
    }
}
