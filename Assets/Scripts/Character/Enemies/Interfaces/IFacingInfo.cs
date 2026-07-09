using UnityEngine;

public interface IFacingInfo : IContext
{
    Transform TransformPosition { get; set; }
    bool IsFacingRight { get; set; }
}
