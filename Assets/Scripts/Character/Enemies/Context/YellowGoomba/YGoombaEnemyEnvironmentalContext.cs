using UnityEngine;

public class YGoombaEnemyEnvironmentalContext : IDirectionInfo, IEdgeInfo, IWallInfo
{
    public Vector3? TargetDirection { get; set; }
    public Transform TransformPosition { get; set; }
    public (float, float) MeshDimensions { get; set; }
    public bool IsEdgeLeft { get; set; }
    public bool IsEdgeRight { get; set; }
    public bool IsTouchingRightWall { get; set; }
    public bool IsTouchingLeftWall { get; set; }

    public YGoombaEnemyEnvironmentalContext(GameObject self)
    {
        this.MeshDimensions = MeshSize.GetSize(self);
    }
}
