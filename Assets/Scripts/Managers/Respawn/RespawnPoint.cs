using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Tooltip("Defaults to this object's position if left empty")]
    public Transform customRespawnLocation;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 respawnPosition = customRespawnLocation ? customRespawnLocation.position : transform.position;
        Gizmos.DrawSphere(respawnPosition, 0.5f);
        Gizmos.DrawLine(transform.position, respawnPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3) RespawnManager.Instance.SetLatestRespawn(this);
    }
}
