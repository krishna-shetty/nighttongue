using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerState : BaseState
{
    protected PlayerController Context;

    protected MoveActionSO ActiveMoveAction => MoveActionOverride ?? Context.CurrentMove;
    protected JumpActionSO ActiveJumpAction => JumpActionOverride ?? Context.CurrentJump;

    protected virtual MoveActionSO MoveActionOverride => null;
    protected virtual JumpActionSO JumpActionOverride => null;

    protected float Gravity;

    public sealed override void EnterState()
    {
        Context.OnActionProfileChanged += HandleProfileChanged;
        InitializeGravity();
        OnEnter();
    }

    public sealed override void ExitState()
    {
        Context.OnActionProfileChanged -= HandleProfileChanged;
        OnExit();
    }

    protected abstract void OnEnter();
    protected abstract void OnExit();
    protected virtual void InitializeGravity()
    {
        Gravity = Context.GravityProfile.CalculateFastFallGravity(ActiveMoveAction, ActiveJumpAction);
    }

    private void HandleProfileChanged(MoveActionSO move, JumpActionSO jump)
    {
        InitializeGravity();
    }
}