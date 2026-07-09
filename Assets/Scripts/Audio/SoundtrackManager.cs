using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(AkGameObj))]

public class SoundtrackManager : MonoBehaviour
{
    private uint id0;
    private uint id1;
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoadedAudio;
        SceneManager.sceneUnloaded += OnSceneUnloadedAudio;
        
        AkBankManager.LoadBank("SoundtrackBank", false, false);
        Debug.Log("SoundtrackBank Loaded");
        AkBankManager.LoadBank("MainSoundBank", false, false);
        Debug.Log("MainSoundBank Loaded");  

    }

    void OnEnable()
    {
        Debug.Log("SoundtrackManager :: OnEnable");
        AkUnitySoundEngine.PostEvent("MusicSwitch_Start", gameObject);
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAudio;
        SceneManager.sceneUnloaded -= OnSceneUnloadedAudio;
    }

    private void OnSceneLoadedAudio(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);   
        switch (scene.buildIndex)
        {
            case 0:
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                AkUnitySoundEngine.PostEvent("MX_State_CRD", gameObject);
                break;
            case 1:
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                AkUnitySoundEngine.PostEvent("MX_State_BAR", gameObject);
                AkUnitySoundEngine.PostEvent("Play_Belly_Bar_Blend_Container", gameObject);
                break;
            case 2:
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                AkUnitySoundEngine.PostEvent("MX_State_TH_EXT", gameObject);
                id0 = AkUnitySoundEngine.PostEvent("Play_Teahouse_Blend_Container", gameObject);
                break;
            case 3:
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                AkUnitySoundEngine.PostEvent("MX_State_TH_INT", gameObject);
                id1 = AkUnitySoundEngine.PostEvent("Play_Teahouse_INT_Blend_Container", gameObject);
                break;
            case 4:
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                AkUnitySoundEngine.PostEvent("MX_State_BOS", gameObject);
                AkUnitySoundEngine.PostEvent("Play_Heart_Blend_Container", gameObject);
                break;
            default:
                Debug.Log("Update SoundtrackManager; there is no case for this scene.");
                break;
        }
    }
    
    private void OnSceneUnloadedAudio(Scene scene)
    {
        switch (scene.buildIndex)
        {
            case 0:
                AkUnitySoundEngine.PostEvent("MX_State_BAR", gameObject);
                break;
            case 1:
                AkUnitySoundEngine.PostEvent("Stop_Belly_Bar_Blend_Container", gameObject);
                break;
            case 2:
                AkUnitySoundEngine.StopPlayingID(id0);
                break;
            case 3:
                AkUnitySoundEngine.StopPlayingID(id1);
                break;
            case 4:
                AkUnitySoundEngine.PostEvent("Stop_Heart_Blend_Container", gameObject);
                Debug.Log("Switching soundtrack on scene: " + scene.name);
                //AkUnitySoundEngine.PostEvent("MX_State_CRD", gameObject);
                break;
            default:
                Debug.Log("Update SoundtrackManager; there is no case for this scene.");
                break;
        }
    }
    private void OnDestroy()
    {
        AkUnitySoundEngine.PostEvent("MusicSwitch_Stop", gameObject);
        AkBankManager.UnloadBank("SoundtrackBank");
        AkBankManager.UnloadBank("MainSoundBank");
    }
}
