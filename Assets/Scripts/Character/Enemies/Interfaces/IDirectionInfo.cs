using UnityEngine;

public interface IDirectionInfo : IContext
{
    Vector3? TargetDirection { get; set; }
    Transform TransformPosition { get; set; }
}
