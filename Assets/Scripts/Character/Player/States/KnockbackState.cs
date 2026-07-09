using UnityEngine;

public class KnockbackState : PlayerState
{
    private MovementHandlerBase _movementHandler;
    private PlayerInputHandler _inputHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    public KnockbackState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
        _movementHandler = MovementHandlerFactory.GetHandler(MovementHandlerType.AIR, controller);
    }

    protected override void OnEnter()
    {
        float initialKnockbackSpeed = Context.KnockbackStrength;
        Context.Velocity = Context.KnockbackDirection * initialKnockbackSpeed; // todo: determine direction of knockback based on position
        Context.HasJumpBuffered = false;
        Context.HasCoyoteBuffered = false;

        GameObject tongue = Context.transform.Find("Tongue").gameObject;
        if (tongue)
        {
            var tongueController = tongue.GetComponent<TongueController>();
            if (tongueController) tongueController.AimTongue();
        }
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        _movementHandler.Apply(Gravity, ActiveMoveAction);
        Context.UpdateBuffers();

        BaseState state = GetNextState();
        if (state != null)
        {
            Context.TransitionToState(state);
        }

        Context.ResetFlags();
    }

    public override BaseState GetNextState()
    {
        if (Context.Velocity.y <= 0f)
        {
            return new FallingState(Context, ActiveMoveAction, ActiveJumpAction);
        }

        return null;
    }

    protected override void OnExit()
    {
    }

    public override void UpdateState()
    {
        //_movementHandler.SimulateStep();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}