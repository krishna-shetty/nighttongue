using UnityEngine;

public class PlatformForceApplier : MonoBehaviour
{
    private MovingPlatform _parentPlatform;
    private BoxCollider _boxCollider;
    private CharacterController _rider;
    private PlayerController _riderController;
    private Transform _riderOriginalParent;

    private void Start()
    {
        _parentPlatform = GetComponent<MovingPlatform>();
        if (_parentPlatform == null)
            _parentPlatform = GetComponentInParent<MovingPlatform>();

        _boxCollider = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        if (_parentPlatform == null) return;

        if (_rider != null)
        {
            if (!IsRiderAbovePlatform())
                DetachRider();
        }
        else
        {
            TryAcquireRider();
        }
    }

    private void TryAcquireRider()
    {
        if (_boxCollider == null) return;

        Vector3 worldCenter = transform.TransformPoint(_boxCollider.center);
        Vector3 worldSize = Vector3.Scale(_boxCollider.size, transform.lossyScale);
        float surfaceY = worldCenter.y + worldSize.y * 0.5f;

        Vector3 overlapCenter = new Vector3(worldCenter.x, surfaceY + 0.4f, worldCenter.z);
        Vector3 halfExtents = new Vector3(worldSize.x * 0.5f, 0.4f, Mathf.Max(worldSize.z * 0.5f, 1f));

        Collider[] hits = Physics.OverlapBox(overlapCenter, halfExtents, Quaternion.identity);
        for (int i = 0; i < hits.Length; i++)
        {
            CharacterController cc = hits[i].GetComponentInParent<CharacterController>();
            if (cc != null)
            {
                _rider = cc;
                _riderController = cc.GetComponent<PlayerController>();
                _riderOriginalParent = _rider.transform.parent;
                _rider.transform.SetParent(transform);
                return;
            }
        }
    }

    private void DetachRider()
    {
        _rider.transform.SetParent(_riderOriginalParent);

        if (_riderController != null)
        {
            Vector3 vel = _parentPlatform.GetCurrentVelocity();
            _riderController.ApplyVelocity(new Vector2(vel.x, vel.y));
        }

        _rider = null;
        _riderController = null;
        _riderOriginalParent = null;
    }

    private bool IsRiderAbovePlatform()
    {
        if (_rider == null || _boxCollider == null) return false;

        Vector3 worldCenter = transform.TransformPoint(_boxCollider.center);
        Vector3 worldSize = Vector3.Scale(_boxCollider.size, transform.lossyScale);
        Vector3 riderPos = _rider.transform.position;

        float halfWidth = worldSize.x * 0.5f + 0.5f;
        if (Mathf.Abs(riderPos.x - worldCenter.x) > halfWidth) return false;

        float surfaceY = worldCenter.y + worldSize.y * 0.5f;
        if (riderPos.y < surfaceY - 0.5f) return false;
        if (riderPos.y > surfaceY + 3f) return false;

        return true;
    }
}
