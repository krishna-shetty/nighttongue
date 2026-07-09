using UnityEngine;

public class PGoombaEnemyInternalState : IFacingInfo, IMoveInfo
{
    public Transform TransformPosition { get; set; }
    public bool IsFacingRight { get; set; }
    public float MoveSpeed { get; set; }
    public Vector3 MoveDirection { get; set; }
    public bool IsAggroed { get; set; }

    public PGoombaEnemyInternalState(float moveSpeed, Vector3 moveDirection)
    {
        MoveSpeed = moveSpeed;
        MoveDirection = moveDirection;
    }
}
