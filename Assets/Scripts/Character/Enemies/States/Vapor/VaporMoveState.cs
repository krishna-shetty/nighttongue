using UnityEngine;

public class VaporMoveState : BaseState
{
    private VaporEnemyInternalState _vaporInternalCtx;
    private VaporEnemyEnvironmentalContext _vaporExternalCtx;
    private VaporAI _vaporAI;

    public VaporMoveState(VaporAI vaporAI)
    {
        _vaporAI = vaporAI;
    }

    public override void UpdateState()
    {
        _vaporAI.SetPosition();
        _vaporAI.UpdateExternalState();
        // Debug.Log($"RightWall: {_vaporExternalCtx.IsTouchingRightWall}, LeftWall: {_vaporExternalCtx.IsTouchingLeftWall}");
        _vaporAI.UpdateInternalState();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void FixedUpdateState() { }
    public override BaseState GetNextState() { return this; }
    public override void EnterState() {
        _vaporInternalCtx = _vaporAI.GetInternalInfo();
        _vaporExternalCtx = _vaporAI.GetExternalInfo();
    }
    public override void ExitState() { }
}
