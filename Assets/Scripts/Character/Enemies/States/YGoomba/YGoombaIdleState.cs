using UnityEngine;

public class YGoombaIdleState : BaseState
{
    private YGoombaEnemyEnvironmentalContext _yGoombaExternalCtx;
    private YellowGoombaAI _yGoombaAI;

    public YGoombaIdleState(YellowGoombaAI yGoombaAI)
    {
        _yGoombaAI = yGoombaAI;
    }

    public override void UpdateState()
    {
        _yGoombaAI.SetPosition();
        _yGoombaAI.UpdateExternalState();
        //Debug.Log($"RightWall: {_vaporExternalCtx.IsTouchingRightWall}, LeftWall: {_vaporExternalCtx.IsTouchingLeftWall}");
        _yGoombaAI.UpdateIdleInternalState();
        if (_yGoombaExternalCtx.TargetDirection.HasValue)
        {
            _yGoombaAI.QueueNextState(new YGoombaChaseState(_yGoombaAI));
        }
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void FixedUpdateState() { }
    public override BaseState GetNextState() { return this; }
    public override void EnterState()
    {
        _yGoombaExternalCtx = _yGoombaAI.GetExternalInfo();
    }
    public override void ExitState() { }
}
