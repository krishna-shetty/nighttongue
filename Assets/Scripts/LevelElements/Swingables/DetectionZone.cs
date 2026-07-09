using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    private GameObject _player;
    private SwingTargetResolver _resolver;
    [SerializeField] private float _maxDetectionAngle = 60f;
    private bool _added = false;

    private void Awake()
    {
        var parent = transform.parent;
        if (parent == null)
        {
            Debug.LogError("DetectionZone must be a child of a swingable object.");
            return;
        }
        _collider = parent.GetComponent<Collider>();
        if (_collider == null)
            Debug.LogError("Swingable parent needs a Collider.");
        else if (_collider.isTrigger)
            Debug.LogError("Swingable parent collider must NOT be a trigger.");
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + (-transform.up), Color.yellow, 2f);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _player = other.gameObject;
        _resolver = _player.GetComponent<SwingTargetResolver>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (_player == null || other.gameObject != _player) return;

        bool inside = IsWithinDownCone(other.transform.position);

        if (inside && !_added)
        {
            _resolver.AddCandidate(_collider);
            _added = true;
        }
        else if (!inside && _added)
        {
            _resolver.RemoveCandidate(_collider);
            _added = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_player == null || other.gameObject != _player) return;

        if (_added)
            _resolver.RemoveCandidate(_collider);

        _player = null;
        _resolver = null;
        _added = false;
    }


    private bool IsWithinDownCone(Vector3 playerWorldPos)
    {
        var sc = GetComponent<SphereCollider>();
        if (sc == null) return false;

        Vector3 center3D = transform.TransformPoint(sc.center);  
        Vector2 center = new Vector2(center3D.x, center3D.y);

        Vector2 player = new Vector2(playerWorldPos.x, playerWorldPos.y);

        Vector2 toTarget = player - center;

        Vector2 extendToPerimeter = toTarget.normalized * GetSphereWorldRadius();

        if (toTarget.sqrMagnitude < 0.0001f)
            return true;

        Vector2 down = Vector2.down;

        float cos = Vector2.Dot(down.normalized, extendToPerimeter.normalized);
        float cosLimit = Mathf.Cos(_maxDetectionAngle * Mathf.Deg2Rad);

        return cos >= cosLimit;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Draw 2D cone in XY plane
        Vector2 down2D = -transform.up;
        float len = GetSphereWorldRadius();
        int segments = 20;

        // Calculate left and right edges of cone
        float leftAngle = -_maxDetectionAngle;
        float rightAngle = _maxDetectionAngle;

        Vector2 leftDir2D = Rotate2D(down2D, leftAngle);
        Vector2 rightDir2D = Rotate2D(down2D, rightAngle);

        Vector3 leftDir = new Vector3(leftDir2D.x, leftDir2D.y, 0);
        Vector3 rightDir = new Vector3(rightDir2D.x, rightDir2D.y, 0);

        // Draw the arc
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;

            float angle1 = leftAngle + t1 * (rightAngle - leftAngle);
            float angle2 = leftAngle + t2 * (rightAngle - leftAngle);

            Vector2 dir1 = Rotate2D(down2D, angle1);
            Vector2 dir2 = Rotate2D(down2D, angle2);

            Vector3 point1 = transform.position + new Vector3(dir1.x, dir1.y, 0) * len;
            Vector3 point2 = transform.position + new Vector3(dir2.x, dir2.y, 0) * len;

            Gizmos.DrawLine(point1, point2);
        }

        // Draw the cone edges
        Gizmos.DrawLine(transform.position, transform.position + leftDir * len);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * len);
    }

    // Helper to rotate a 2D vector by angle (in degrees)
    private Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private float GetSphereWorldRadius()
    {
        var sc = GetComponent<SphereCollider>();
        if (sc == null) return 1f;
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        return sc.radius * maxScale;
    }
}