using UnityEngine;

public class TurnActuator : IActuator
{
    private IWallInfo _externalWallCtx;
    private IEdgeInfo _externalEdgeCtx;
    public TurnActuator(IWallInfo extenalWallContext, IEdgeInfo externalEdgeCtx)
    {
        _externalWallCtx = extenalWallContext;
        _externalEdgeCtx = externalEdgeCtx;
    }
    public void Act(IContext context)
    {
        if (context is IMoveInfo moveInfo)
        {
            if (_externalWallCtx.IsTouchingLeftWall)
            {
                Vector3 NewDirection = moveInfo.MoveDirection;
                NewDirection.x = Mathf.Abs(moveInfo.MoveDirection.x);
                moveInfo.MoveDirection = NewDirection;
            }
            else if (_externalWallCtx.IsTouchingRightWall)
            {
                Vector3 NewDirection = moveInfo.MoveDirection;
                NewDirection.x = -Mathf.Abs(moveInfo.MoveDirection.x);
                moveInfo.MoveDirection = NewDirection;
            }
        }
    }
}
