using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class RespawnTrigger : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event respawnSound;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3) RespawnManager.Instance.RespawnPlayerAtLastPoint();
        
        if (respawnSound != null && respawnSound.IsValid()) respawnSound.Post(gameObject);
    }
}
