using UnityEngine;

public class SphereCollisionShape : MonoBehaviour, ICollisionShape
{
    private float _radius;

    public SphereCollisionShape(float radius)
    {
        _radius = radius;
    }

    public bool Cast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, out RaycastHit hit)
    {
        return Physics.SphereCast(
            transform.position,
            _radius,
            direction,
            out hit,
            distance,
            layerMask
        );
    }

    private void Start()
    {
        MeshFilter sphereMeshFilter = GetComponent<MeshFilter>();
        if (sphereMeshFilter != null)
        {
            Bounds meshBounds = sphereMeshFilter.sharedMesh.bounds;
            // local space bounds
            Vector3 extents = meshBounds.extents;
            // half-size in axis
            Vector3 lossy = transform.lossyScale;
            float scaledX = extents.x * lossy.x;
            float scaledY = extents.y * lossy.y;
            float scaledZ = extents.z * lossy.z;
            _radius = Mathf.Max(scaledX, scaledY, scaledZ);
        }
    }
}
