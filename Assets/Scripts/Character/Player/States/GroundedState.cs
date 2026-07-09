using UnityEngine;

public class GroundedState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;
    private MovementHandlerBase _movementHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    private int fallingFrameCounter = 0;    // 1 frame delay before you can go to falling state
                                            // avoids state thrashing when running on a slope.

    public GroundedState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
        _movementHandler = MovementHandlerFactory.GetHandler(MovementHandlerType.GROUNDED, controller);
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

        fallingFrameCounter = 0;
        Context.RaiseGroundedStart();
    }

    public override BaseState GetNextState()
    {
        // Check for jump request or buffered jump
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context, ActiveMoveAction, ActiveJumpAction);
        }
        if (!Context.GroundDetector.IsGrounded)
        {
            ++fallingFrameCounter;
            if (fallingFrameCounter > 1)
                return new FallingState(Context, ActiveMoveAction, ActiveJumpAction);
        }
        else
        {
            fallingFrameCounter = 0; // reset counter if still grounded
        }
        return null;
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

    protected override void OnExit()
    {
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformRequest;
        }
        // Clear jump flags when leaving ground
        Context.RequestedJump = false;
        Context.HasCoyoteBuffered = false;
        Context.HasJumpBuffered = false;
    }

    private void HandleTransformRequest(TongueTransformEventArgs args)
    {
        if (args.IsTransformed)
        {
            // entering ball
            dynamicMoveAction = args.MoveAction;
            dynamicJumpAction = args.JumpAction;
        }
        else
        {
            // exiting ball
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