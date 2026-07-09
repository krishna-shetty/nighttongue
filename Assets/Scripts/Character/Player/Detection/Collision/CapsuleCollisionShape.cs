using UnityEngine;

public class CapsuleCollisionShape  : MonoBehaviour, ICollisionShape
{
    private Vector3 _point1;
    private Vector3 _point2;
    private float _radius;

    public CapsuleCollisionShape(Vector3 point1, Vector3 point2, float radius)
    {
        _point1 = point1;
        _point2 = point2;
        _radius = radius;
    }

    public bool Cast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit)
    {
        return Physics.CapsuleCast(
            origin + _point1,
            origin + _point2,
            _radius,
            direction,
            out hit,
            distance,
            layerMask
        );
    }
    
    public void SetCapsuleParameters(Vector3 point1, Vector3 point2, float radius)
    {
        _point1 = point1;
        _point2 = point2;
        _radius = radius;
    }
}
