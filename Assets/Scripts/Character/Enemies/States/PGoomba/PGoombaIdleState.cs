using UnityEngine;

public class PGoombaIdleState : BaseState
{
    private PGoombaEnemyInternalState _pGoombaInternalCtx;
    private PGoombaEnemyEnvironmentalContext _pGoombaExternalCtx;
    private PurpleGoombaAI _pGoombaAI;

    public PGoombaIdleState(PurpleGoombaAI pGoombaAI)
    {
        _pGoombaAI = pGoombaAI;
    }

    public override void UpdateState()
    {
        _pGoombaAI.SetPosition();
        _pGoombaAI.UpdateExternalState();
        //Debug.Log($"RightWall: {_vaporExternalCtx.IsTouchingRightWall}, LeftWall: {_vaporExternalCtx.IsTouchingLeftWall}");
        _pGoombaAI.UpdateIdleInternalState();
        if (_pGoombaExternalCtx.TargetDirection.HasValue)
        {
            _pGoombaAI.QueueNextState(new PGoombaChaseState(_pGoombaAI));
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void FixedUpdateState() { }
    public override BaseState GetNextState() { return this; }
    public override void EnterState()
    {
        _pGoombaInternalCtx = _pGoombaAI.GetInternalInfo();
        _pGoombaExternalCtx = _pGoombaAI.GetExternalInfo();
    }
    public override void ExitState() { }
}
