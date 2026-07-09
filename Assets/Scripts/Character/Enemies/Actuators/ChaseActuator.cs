using UnityEngine;

public class ChaseActuator : IActuator
{
    private IDirectionInfo _externalDirectionCtx;
    public ChaseActuator(IDirectionInfo externalDirectionCtx)
    {
        _externalDirectionCtx = externalDirectionCtx;

    }
    public void Act(IContext context)
    {
        if (context is IMoveInfo moveInfo)
        {
            if(_externalDirectionCtx.TargetDirection.HasValue)
            {
                if(_externalDirectionCtx.TargetDirection.Value.x > 0)
                {
                    Vector3 NewDirection = moveInfo.MoveDirection;
                    NewDirection.x = Mathf.Abs(moveInfo.MoveDirection.x);
                    moveInfo.MoveDirection = NewDirection;
                }
                else
                {
                    Vector3 NewDirection = moveInfo.MoveDirection;
                    NewDirection.x = -Mathf.Abs(moveInfo.MoveDirection.x);
                    moveInfo.MoveDirection = NewDirection;
                }
            }
        }
    }
}

