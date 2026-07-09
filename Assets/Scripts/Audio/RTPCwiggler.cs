using UnityEngine;

public class RTPCwiggler : MonoBehaviour
{
    public string[] rtpcName;

    public Vector3[] bounds;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < rtpcName.Length; i++)
        {
            AkUnitySoundEngine.SetRTPCValue(rtpcName[i], bounds[i].y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < rtpcName.Length; i++)
        {
            AkUnitySoundEngine.SetRTPCValue(rtpcName[i], Random.Range(bounds[i].x, bounds[i].z));
        }
    }
}
