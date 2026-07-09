using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class JumpingState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;
    private MovementHandlerBase _movementHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    private float _jumpGravity;
    private float _fastFallGravity;
    private float _initialJumpSpeed;

    public JumpingState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
        _movementHandler = MovementHandlerFactory.GetHandler(MovementHandlerType.AIR, controller);
    }

    protected override void InitializeGravity()
    {
        _jumpGravity = Context.GravityProfile.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        _fastFallGravity = Context.GravityProfile.CalculateFastFallGravity(ActiveMoveAction, ActiveJumpAction);
        Gravity = _jumpGravity;
    }

    private void ApplyJump()
    {
        Context.Velocity.y = _initialJumpSpeed;
    }

    private void HandleJumpLogic()
    {
        ApplyJump();
        Context.HasJumpBuffered = false;
        Context.HasCoyoteBuffered = false;
    }

    public override BaseState GetNextState()
    {
        if (Context.Velocity.y <= 0f)
        {
            return new FallingState(Context, ActiveMoveAction, ActiveJumpAction);
        }

        return null;
    }

    protected override void OnEnter()
    {
        Context.RaiseJumpStarted();
        _initialJumpSpeed = Context.GravityProfile.CalculateInitialJumpSpeed(ActiveMoveAction, ActiveJumpAction);

        _tongueTransformHandler = Context.GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler == null)
        {
            Debug.LogError("JumpingState :: TongueTransformHandler component not found on PlayerController.");
            return;
        }
        _tongueTransformHandler.OnTransformStateChanged += HandleTransformStateChange;

        HandleJumpLogic();
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        _movementHandler.Apply(Gravity, ActiveMoveAction);
        Context.UpdateBuffers();

        if (!Context.IsJumpHeld)
        {
            Context.Velocity.y *= ActiveJumpAction.jumpCutoffMultiplier;
            Context.TransitionToState(new FallingState(Context, ActiveMoveAction, ActiveJumpAction));
            return;
        }

        BaseState state = GetNextState();
        if (state != null)
        {
            Context.TransitionToState(state);
        }
        Context.ResetFlags();
    }

    protected override void OnExit()
    {
    }

    private void HandleTransformStateChange(TongueTransformEventArgs args)
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

    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}