using UnityEngine;

public class MoveActuator : IActuator
{
    public void Act(IContext context)
    {
        if (context is IMoveInfo moveInfo)
        {
            Vector3 newPosition = (moveInfo.MoveSpeed * moveInfo.MoveDirection * Time.deltaTime) + moveInfo.TransformPosition.position;
            //moveInfo.TransformPosition.position = moveLength * Time.fixedDeltaTime;
            moveInfo.TransformPosition.position = newPosition;
        }
    }
}