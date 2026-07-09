using UnityEngine;

public class SetCurrentCheckpoint : MonoBehaviour
{
    [SerializeField]
    private int checkpointId = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PlayerModel"))
        {
            if(SaveManager.Instance != null)
            {
                SaveManager.Instance.currCheckpoint = checkpointId;
            }
        }
    }
}
