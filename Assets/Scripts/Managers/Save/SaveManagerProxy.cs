using UnityEngine;

public class SaveManagerProxy : MonoBehaviour
{
    public void SaveLastBarCutscene(bool value)
    {
        SaveManager.Instance.SaveLastBarCutscene(value);
    }
}
