using Unity.Mathematics;
using UnityEngine;

public class GrapplingMovementHandler : MovementHandlerBase
{
    public GrapplingMovementHandler(PlayerController controller) : base(controller)
    {
        if (Context && Context.StateMultiplier) stateMultiplier = Context.StateMultiplier.Grappling;
    }

    public override void Apply(float gravity, MoveActionSO move)
    {
        if (Context == null)
        {
            Debug.LogWarning("[GrapplingMovementHandler] Context is null; skipping Apply.");
            return;
        }

        move ??= Context.MoveAction;

        ApplyExternalForces(stateMultiplier);
        ApplyVerticalMovement(gravity);
        SimulateStep();
    }
}