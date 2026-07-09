using Unity.Mathematics;
using UnityEngine;

public class SwingingMovementHandler : MovementHandlerBase
{
    private SwingingState _state; // for reference to state variables i.e. swingPoint

    public SwingingMovementHandler(PlayerController controller) : base(controller)
    {
    }

    public override void Apply(float gravity, MoveActionSO move)
    {
        if (Context == null)
        {
            Debug.LogWarning("[SwingingMovementHandler] Context is null; skipping Apply.");
            return;
        }

        move ??= Context.MoveAction;
        if (move == null)
        {
            Debug.LogWarning("[SwingingMovementHandler] MoveAction is null; skipping Apply.");
            return;
        }

        if (_state == null)
        {
            Debug.LogWarning("[SwingingMovementHandler] _state is null; call SetSwingingState(...) in SwingingState.OnEnter.");
            SimulateStep();
            return;
        }

        ApplyExternalForces(stateMultiplier);
        ApplySwingPlayerInput();
        ApplyPendulumPhysics();
        ApplySwingDamping();
        SimulateStep();
    }

    private void ApplyPendulumPhysics()
    {
        if (_state == null || Context?.Settings == null) return;

        Vector3 swingPoint = _state.GetSwingPoint();
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - swingPoint;

        float swingRestLength = _state.GetSwingRestLength();
        float currentLen = ropeVec.magnitude;

        if (currentLen < Context.Settings.FloatPrecisionThreshold) return;

        Vector3 ropeDir = ropeVec / currentLen;
        Vector3 tangentialDir = new(-ropeDir.y, ropeDir.x, 0); // unused variable?
        Vector3 velocity = new(Context.Velocity.x, Context.Velocity.y, 0);

        float radialVel = Vector3.Dot(velocity, ropeDir);
        if (radialVel > 0)
        {
            velocity -= ropeDir * radialVel;
        }

        velocity += Physics.gravity * Time.fixedDeltaTime;

        Vector3 nextPos = playerPos + velocity * Time.fixedDeltaTime;

        Vector3 nextRopeVec = nextPos - swingPoint;
        float nextLen = nextRopeVec.magnitude;

        if (Mathf.Abs(nextLen - swingRestLength) > Context.Settings.FloatPrecisionThreshold)
        {
            nextPos = swingPoint + nextRopeVec.normalized * swingRestLength;
            velocity = (nextPos - playerPos) / Time.fixedDeltaTime;
        }

        Context.Velocity.x = velocity.x;
        Context.Velocity.y = velocity.y;
    }

    public void ApplySwingPlayerInput()
    {
        if (_state == null) return;

        SwingAbilitySO activeAbility = _state.GetActiveAbilitySO();
        Vector3 swingPoint = _state.GetSwingPoint();
        Vector3 playerPos = Context.transform.position;

        Vector3 ropeVec = playerPos - swingPoint;
        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new(-ropeDir.y, ropeDir.x, 0);

        float input = Context.HorizontalInput * activeAbility.UserControlForce;
        Vector3 inputForce = tangentialDir * input;

        Context.Velocity.x += inputForce.x * Time.fixedDeltaTime;
        Context.Velocity.y += inputForce.y * Time.fixedDeltaTime;
    }

    private void ApplySwingDamping()
    {
        if (_state == null || Context?.Settings == null) return;

        SwingAbilitySO activeAbility = _state.GetActiveAbilitySO();
        Vector3 swingPoint = _state.GetSwingPoint();
        Vector3 ropeVec = Context.transform.position - swingPoint;
        float ropeLen = ropeVec.magnitude;

        if (ropeLen > Context.Settings.FloatPrecisionThreshold)
        {
            Vector3 ropeDir = ropeVec / ropeLen;

            Vector3 vel = new(Context.Velocity.x, Context.Velocity.y, 0);
            float radialVel = Vector3.Dot(vel, ropeDir);
            Vector3 tangentialVel = vel - ropeDir * radialVel;

            float dampingFactor = 1f - (activeAbility.Damping * Time.fixedDeltaTime);
            dampingFactor = Mathf.Clamp01(dampingFactor);

            Vector3 dampedTangential = tangentialVel * dampingFactor;
            Vector3 finalVel = dampedTangential + ropeDir * radialVel;

            Context.Velocity.x = finalVel.x;
            Context.Velocity.y = finalVel.y;
        }
    }

    // unused
    private void ApplySpringRopeSwingConstraints()
    {
        SwingAbilitySO activeAbility = _state.GetActiveAbilitySO();
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _state.GetSwingPoint();
        float currentLen = ropeVec.magnitude;
        Vector3 direction = ropeVec.normalized;

        float targetLen = _state.GetSwingRestLength();
        float lengthDiff = currentLen - targetLen;

        Vector3 springForce = Vector3.zero;
        float springConstant = activeAbility.SpringConstant;

        if (lengthDiff > 0)
        {
            springForce = -springConstant * lengthDiff * direction;
        }
        else
        {
            springForce = Vector3.zero;
        }

        Vector3 gravity = Physics.gravity;
        Vector3 totalForce = springForce + gravity;

        Context.Velocity.x += totalForce.x * Time.fixedDeltaTime;
        Context.Velocity.y += totalForce.y * Time.fixedDeltaTime;
    }

    public void SetSwingingState(SwingingState state) { _state = state; }
}