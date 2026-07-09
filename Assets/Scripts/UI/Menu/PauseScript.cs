using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PauseScript : MonoBehaviour
{
    public void BackButton()
    {
        SettingMenu.Instance.BackButton();
    }

    public void SettingsButton()
    {
        SettingMenu.Instance.SettingsButton();
    }

    public void QuitButton()
    {
        SettingMenu.Instance.QuitButton();
    }

    public void CheckpointButton()
    {
        SettingMenu.Instance.CheckpointButton();
    }
}