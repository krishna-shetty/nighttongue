using UnityEngine;

public interface ICollisionShape
{
    bool Cast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit);
}


