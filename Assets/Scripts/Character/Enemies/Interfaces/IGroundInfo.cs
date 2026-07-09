using UnityEngine;

public interface IGroundInfo : IContext
{
    bool IsGrounded { get; set; }
    Transform TransformPosition { get; set; }
}
