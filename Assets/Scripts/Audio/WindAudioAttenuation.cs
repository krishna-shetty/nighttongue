using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WindAudioAttenuation : MonoBehaviour
{
    private WindVolume[] _windVolumes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateWindVolumeList();
    }

    // Update is called once per frame
    void Update()
    {
        AkSoundEngine.SetRTPCValue("windIntensity", Vector3.Distance(transform.position, GetClosest()));
    }

    private void CreateWindVolumeList()
    {
       _windVolumes = FindObjectsByType<WindVolume>(FindObjectsSortMode.None);
    }
    private Vector3 GetClosest()
    {
        Vector3 closestVector = Vector3.zero;
        float minSquareDistance = Mathf.Infinity;

        foreach (WindVolume windVolume in _windVolumes)
        {
            float squareDistance = (windVolume.transform.position - transform.position).sqrMagnitude;

            if (squareDistance < minSquareDistance)
            {
                minSquareDistance = squareDistance;
                closestVector = windVolume.transform.position;
            }
        }
        return closestVector;
    }
}
