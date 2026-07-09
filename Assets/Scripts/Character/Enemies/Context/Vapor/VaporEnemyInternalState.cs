using UnityEngine;

public class VaporEnemyInternalState : IMoveInfo
{
    public Transform TransformPosition { get; set; }
    public float MoveSpeed { get; set; }
    public Vector3 MoveDirection { get; set; }

    public VaporEnemyInternalState(float moveSpeed, Vector3 moveDirection)
    {
        MoveSpeed = moveSpeed;
        MoveDirection = moveDirection;
    }
}
