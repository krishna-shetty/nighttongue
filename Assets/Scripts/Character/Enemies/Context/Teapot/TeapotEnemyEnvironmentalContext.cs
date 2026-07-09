using UnityEngine;

public class TeapotEnemyEnvironmentalContext : IDirectionInfo
{
    public Vector3? TargetDirection { get; set; }
    public Transform TransformPosition { get; set; }
}