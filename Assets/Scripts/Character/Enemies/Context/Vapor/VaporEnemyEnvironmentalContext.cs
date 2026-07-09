using UnityEngine;

public class VaporEnemyEnvironmentalContext : IWallInfo
{
    public bool IsTouchingRightWall { get; set; }
    public bool IsTouchingLeftWall { get; set; }
    public Transform TransformPosition { get; set; }
}