using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class PlaySound : MonoBehaviour
{
    public AK.Wwise.Event[] sounds;

    public void Play(int index = 0)
    {
        AkUnitySoundEngine.PostEvent(sounds[index].Id, gameObject);
    }

    public void PlayAll()
    {
        foreach (AK.Wwise.Event sound in sounds)
            AkUnitySoundEngine.PostEvent(sound.Id, gameObject);
    }
}
