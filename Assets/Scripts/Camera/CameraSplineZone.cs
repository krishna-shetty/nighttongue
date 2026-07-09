using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(BoxCollider))]
public class CameraSplineZone : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineCamera zoneCamera;
    [SerializeField] private CinemachineSplineDolly splineDolly;

    [Header("Progress Mapping (player -> spline t)")]
    [SerializeField] private Axis progressAxis = Axis.X; // X for 2.5D, Z for 3D-forward
    [SerializeField] private float padding = 0f;         // avoids snapping near edges
    [SerializeField] private float lerpSpeed = 8f;       // smoothing

    [Header("Switching")]
    [SerializeField] private int zonePriority = 30;

    private BoxCollider box;
    private bool inside;
    private int prevPriority;
    private float tSmoothed;

    private void Awake()
    {

        box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        if (splineDolly == null)
            splineDolly = zoneCamera.GetComponent<CinemachineSplineDolly>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        inside = true;

        prevPriority = zoneCamera.Priority;
        zoneCamera.Priority = zonePriority;

        // Initialize smoothing to current value so it doesn't jump
        tSmoothed = Mathf.Clamp01(GetProgress01());
        splineDolly.CameraPosition = tSmoothed;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        inside = false;
        zoneCamera.Priority = prevPriority;
    }

    private void LateUpdate()
    {
        if (!inside || player == null || zoneCamera == null || splineDolly == null) return;

        float t = Mathf.Clamp01(GetProgress01());

        tSmoothed = Mathf.Lerp(tSmoothed, t, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));

        // CameraPosition expects 0..1
        splineDolly.CameraPosition = tSmoothed;
    }

    private float GetProgress01()
    {
        // Convert player to zone-local space
        Vector3 local = transform.InverseTransformPoint(player.position);

        Vector3 half = box.size * 0.5f;
        Vector3 center = box.center;

        float min, max, value;

        switch (progressAxis)
        {
            case Axis.X:
                min = center.x - half.x + padding;
                max = center.x + half.x - padding;
                value = local.x;
                break;
            case Axis.Y:
                min = center.y - half.y + padding;
                max = center.y + half.y - padding;
                value = local.y;
                break;
            default:
                min = center.z - half.z + padding;
                max = center.z + half.z - padding;
                value = local.z;
                break;
        }

        if (Mathf.Abs(max - min) < 0.0001f) return 0f;
        return Mathf.InverseLerp(min, max, value);
    }

    private bool IsPlayer(Collider other)
    {
        return other.transform == player || other.transform.root == player;
    }
}
