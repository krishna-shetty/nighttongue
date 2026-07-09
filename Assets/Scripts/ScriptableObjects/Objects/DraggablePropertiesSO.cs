using UnityEngine;

public enum DraggableType
{
    WindAffected,
    WindImmune
}

[CreateAssetMenu(fileName = "Throwable", menuName = "Objects/Throwable")]
public class DraggablePropertiesSO : ScriptableObject
{
    public DraggableType DragType = DraggableType.WindAffected;

    public bool DoGroundCheck = true;

    public float Friction = 0.2f;
}
