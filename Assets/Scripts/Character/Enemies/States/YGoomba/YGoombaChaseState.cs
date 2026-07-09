using UnityEngine;

public class YGoombaChaseState : BaseState
{
    private YellowGoombaAI _yGoombaAI;

    public YGoombaChaseState(YellowGoombaAI yGoombaAI)
    {
        _yGoombaAI = yGoombaAI;
    }

    public override void UpdateState()
    {
        _yGoombaAI.SetPosition();
        _yGoombaAI.UpdateExternalState();
        //Debug.Log($"RightWall: {_vaporExternalCtx.IsTouchingRightWall}, LeftWall: {_vaporExternalCtx.IsTouchingLeftWall}");
        _yGoombaAI.UpdateChaseInternalState();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void FixedUpdateState() { }
    public override BaseState GetNextState() { return this; }
    public override void EnterState() { }
    public override void ExitState() { }
}
