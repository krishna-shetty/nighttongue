using UnityEngine;

public class StopActuator : IActuator
{
    private IWallInfo _externalWallCtx;
    private IEdgeInfo _externalEdgeCtx;
    private float _moveSpeed;
    public StopActuator(IWallInfo externalWallCtx, IEdgeInfo externalEdgeCtx, float moveSpeed)
    {
        _externalEdgeCtx = externalEdgeCtx;
        _externalWallCtx = externalWallCtx;
        _moveSpeed = moveSpeed;
    }
    public void Act(IContext context)
    {
        if (context is IMoveInfo moveInfo)
        {
            if(moveInfo.MoveDirection.x > 0)
            {
                if(_externalEdgeCtx.IsEdgeRight || _externalWallCtx.IsTouchingRightWall)
                {
                    moveInfo.MoveSpeed = 0f;
                }
                else
                {
                    moveInfo.MoveSpeed = _moveSpeed;
                }
            }
            else
            {
                if (_externalEdgeCtx.IsEdgeLeft || _externalWallCtx.IsTouchingLeftWall)
                {
                    moveInfo.MoveSpeed = 0f;
                }
                else
                {
                    moveInfo.MoveSpeed = _moveSpeed;
                }
            }
                

        }
    }
}

