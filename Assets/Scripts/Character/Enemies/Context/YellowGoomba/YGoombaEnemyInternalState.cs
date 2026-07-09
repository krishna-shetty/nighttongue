using UnityEngine;

public class YGoombaEnemyInternalState : IFacingInfo, IMoveInfo, IDangerInfo
{
    public Transform TransformPosition { get; set; }
    public bool IsFacingRight { get; set; }
    public float MoveSpeed { get; set; }
    public Vector3 MoveDirection { get; set; }
    public bool IsAggroed { get; set; }
    public bool isDanger { get; set; }

    public YGoombaEnemyInternalState(float moveSpeed, Vector3 moveDirection)
    {
        MoveSpeed = moveSpeed;
        MoveDirection = moveDirection;
        isDanger = false;
    }
}
