using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 0;   
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
