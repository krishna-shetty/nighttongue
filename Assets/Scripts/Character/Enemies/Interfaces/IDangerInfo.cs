using UnityEngine;

public interface IDangerInfo : IContext
{
    Transform TransformPosition { get; set; }
    bool isDanger { get; set; }
}
