using UnityEngine;

public interface IWallInfo : IContext
{
    bool IsTouchingRightWall { get; set; }
    bool IsTouchingLeftWall { get; set; }
    Transform TransformPosition { get; set; }
}
