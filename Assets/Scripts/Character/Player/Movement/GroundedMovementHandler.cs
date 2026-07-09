using UnityEngine;
using Unity.Mathematics;

public class GroundedMovementHandler : MovementHandlerBase
{
    public GroundedMovementHandler(PlayerController controller) : base(controller)
    {
        if (Context && Context.StateMultiplier) stateMultiplier = Context.StateMultiplier.Grounded;
    }

    public override void Apply(float gravity, MoveActionSO move)
    {
        if (Context == null)
        {
            Debug.LogWarning("[GroundedMovementHandler] Context is null; skipping Apply.");
            return;
        }

        move ??= Context.MoveAction;
        if (move == null)
        {
            Debug.LogWarning("[GroundedMovementHandler] MoveAction is null; skipping Apply.");
            return;
        }

        ApplyExternalForces(stateMultiplier);
        ApplyVerticalMovement(gravity);

        if (Context.UsingPrecisionMovement())
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
            ApplySpeedDecay(move);
            ApplyFriction(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
        SimulateStep();

        if (Context.GroundDetector.IsGrounded)
        {   // do not let ground push you up... the ground is pushing the player up off the ground
            // this kicks the player into fall state when running up slopes... we don't want that
            Context.Velocity.y = Mathf.Min(Context.Velocity.y, 0.0f);
        }
    }
}