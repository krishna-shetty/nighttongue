using UnityEngine;

public interface IEdgeInfo : IContext
{
    Transform TransformPosition { get; set; }
    (float, float) MeshDimensions { get; set; }
    bool IsEdgeLeft { get; set; }
    bool IsEdgeRight { get; set; }
}
