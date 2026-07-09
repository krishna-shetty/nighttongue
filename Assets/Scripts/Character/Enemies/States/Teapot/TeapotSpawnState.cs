using UnityEngine;

public class TeapotSpawnState : BaseState
{
    private TeapotEnemyInternalState _teapotInternalCtx;
    private TeapotEnemyEnvironmentalContext _teapotExternalCtx;
    private TeapotAI _teapotAI;

    public TeapotSpawnState(TeapotAI teapotAI)
    {
        _teapotAI = teapotAI;
    }

    public override void UpdateState()
    {
        _teapotAI.SetPosition();
        _teapotAI.UpdateExternalState();
        //Debug.Log($"RightWall: {_vaporExternalCtx.IsTouchingRightWall}, LeftWall: {_vaporExternalCtx.IsTouchingLeftWall}");
        _teapotAI.UpdateInternalState();
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void FixedUpdateState() { }
    public override BaseState GetNextState() { return this; }
    public override void EnterState() {
        _teapotInternalCtx = _teapotAI.GetInternalInfo();
        _teapotExternalCtx = _teapotAI.GetExternalInfo();
    }
    public override void ExitState() { }
}
