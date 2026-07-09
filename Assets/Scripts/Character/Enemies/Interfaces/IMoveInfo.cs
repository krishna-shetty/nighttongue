using UnityEngine;

public interface IMoveInfo : IContext
{
    Transform TransformPosition { get; set; }
    float MoveSpeed { get; set; }
    Vector3 MoveDirection { get; set; }
}
