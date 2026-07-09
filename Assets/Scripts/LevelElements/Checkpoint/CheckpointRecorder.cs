using UnityEngine;

public class CheckpointRecorder : MonoBehaviour
{
    [SerializeField]
    private int CheckpointNumber;
    [SerializeField]
    private PromptCutscene _cutscene; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cutscene = GetComponentInChildren<PromptCutscene>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerModel"))
        {
            return;
        }
        RegisterCheckpoint();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerModel"))
        {
            return;
        }
        UnregisterCheckpoint();
    }

    private void RegisterCheckpoint()
    {
        if (SaveManager.Instance != null)
        {
            Debug.Log("CheckpointRecorder :: Saving OnTriggerEnter");
            SaveManager.Instance.currCheckpoint = CheckpointNumber;
            SaveManager.Instance.SaveGame(CheckpointNumber);

            _cutscene?.EnableCanvas(true);
        }
        else Debug.LogError("CheckpointRecorder :: Could not register checkpoint, no SaveManager found");
    }

    private void UnregisterCheckpoint()
    {
        _cutscene?.EnableCanvas(false);
    }
}
