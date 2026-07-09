using UnityEngine;

public class TongueTipCollider : MonoBehaviour
{
    [Header("Target layer to detect collisions with")]
    public LayerMask targetLayer;

    [Header("Reference to the stretch script (on tongue root)")]
    public TongueJointStretch tongueJointStretch;

    [Header("connect with tongue controller ")]
    public TongueController tongueController;


    void OnCollisionEnter(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject))
        {
            Debug.Log($"[TongueTip] Collision with: {collision.gameObject.name}");

            if (tongueJointStretch != null)
            {
                // Get the collision point in world space
                Vector3 hitPoint = collision.contacts[0].point;

                // Get distance along the stretch axis
                float distance = 0f;
                Vector3 rootPos = tongueJointStretch.transform.position;
                Vector3 localHit = tongueJointStretch.transform.InverseTransformPoint(hitPoint);

                switch (tongueJointStretch.stretchAxis)
                {
                    case TongueJointStretch.StretchAxis.X:
                        distance = Mathf.Abs(localHit.x);
                        break;
                    case TongueJointStretch.StretchAxis.Y:
                        distance = Mathf.Abs(localHit.y);
                        break;
                    case TongueJointStretch.StretchAxis.Z:
                        distance = Mathf.Abs(localHit.z);
                        break;
                }

                tongueJointStretch.LimitStretchAtCollision(distance);
            }
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
