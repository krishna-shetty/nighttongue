using UnityEngine;

public class FallingState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;
    private MovementHandlerBase _movementHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;
    private float stateStartTime = -1.0f;
    private const float maxFallTime = 3.0f; // maximum time you can fall before instant death

    public FallingState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
        _movementHandler = MovementHandlerFactory.GetHandler(MovementHandlerType.AIR, controller);
    }

    protected override void OnEnter()
    {
        _tongueTransformHandler = Context.GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler == null)
        {
            Debug.LogError("GroundedState :: TongueTransformHandler component not found on PlayerController.");
            return;
        }
        _tongueTransformHandler.OnTransformStateChanged += HandleTransformRequest;

        Context.ActivateCoyoteTime();
        stateStartTime = Time.realtimeSinceStartup;
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        _movementHandler.Apply(Gravity, ActiveMoveAction);
        Context.UpdateBuffers();

        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.TransitionToState(nextState);
        }

        Context.ResetFlags();
    }

    public override BaseState GetNextState()
    {
        if (Context.GroundDetector.IsGrounded)
        {
            return new GroundedState(Context);
        }

        // Check for coyote jump
        if (Context.HasCoyoteBuffered && Context.RequestedJump)
        {
            return new JumpingState(Context);
        }

#if false   // doesn't work with floating wind mechanic
        float fallTime = Time.realtimeSinceStartup - stateStartTime;
        if (fallTime > maxFallTime)
        {
            Debug.LogError("Falling death timer!");
            DamageReceiver damage = Context.GetComponent<DamageReceiver>();
            if (damage != null)
            {
                damage.ApplyDamage(9999, Context.gameObject); // Apply lethal damage
            }
        }
#endif

        return null; // Stay in FallingState
    }

    protected override void OnExit()
    {
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformRequest;
        }
    }

    private void HandleTransformRequest(TongueTransformEventArgs args)
    {
        if (args.IsTransformed)
        {
            dynamicMoveAction = args.MoveAction;
            dynamicJumpAction = args.JumpAction;
        }
        else
        {
            dynamicMoveAction = null;
            dynamicJumpAction = null;
        }
        InitializeGravity();
    }

    public override void UpdateState()
    {
        //_movementHandler.SimulateStep();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}