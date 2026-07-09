using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(AkGameObj))]

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event sound;
    [SerializeField] private bool stopOnDisable = true;
    [SerializeField] private int stopTime = 1;
    
    private bool isBankLoaded = false;
    
    void OnEnable()
    {
        if (!isBankLoaded) return;
        sound.Post(gameObject);
    }
    void OnDisable()
    {
        sound.Stop(gameObject, stopTime);
    }

    void Start()
    {
        isBankLoaded = true;
    }

}
