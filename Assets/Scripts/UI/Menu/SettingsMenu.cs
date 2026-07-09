using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public enum SupportedResolutions
    {
        R1920x1080,
        R1920x1200,
        R1280x720
    }
    [SerializeField]
    private GameObject toggle;
    private Image _toggleSprite;
    [SerializeField]
    private Sprite fullScreenOn, fullScreenOff;
    [SerializeField]
    public FullScreenMode fullscreenStatus = FullScreenMode.Windowed;
    [SerializeField]
    private TMP_Dropdown ResolutionDropdown;
    public SupportedResolutions CurrentResolution;
    private string _savePath;
    public GameObject MainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _toggleSprite = toggle.GetComponent<Image>();
        CurrentResolution = SupportedResolutions.R1920x1080;
        string[] resolutionNames = Enum.GetNames(typeof(SupportedResolutions));
        List<string> resolutionList = new List<string>(resolutionNames);
        ResolutionDropdown.ClearOptions();
        ResolutionDropdown.AddOptions(resolutionList);
        _savePath = Application.persistentDataPath + "/save.json";
    }

    public void OnDropdownChanged(int index)
    {
        CurrentResolution = (SupportedResolutions)index;
        ChangeResolution();
    }

    public void ChangeResolution()
    {
        switch (CurrentResolution)
        {
            case (SupportedResolutions.R1920x1080):
                {
                    Debug.Log("resolution changed to: " + "1920 x 1080 with " + fullscreenStatus);
                    Screen.SetResolution(1920, 1080, fullscreenStatus);
                    break;
                }
            case (SupportedResolutions.R1920x1200):
                {
                    Debug.Log("resolution changed to: " + "1920 x 1200 with " + fullscreenStatus);
                    Screen.SetResolution(1920, 1200, fullscreenStatus);
                    break;
                }
            case (SupportedResolutions.R1280x720):
                {
                    Debug.Log("resolution changed to: " + "1280 x 720 with " + fullscreenStatus);
                    Screen.SetResolution(1280, 720, fullscreenStatus);
                    break;
                }
            default:
                {
                    Debug.Log("how did you select this resolution??");
                    break;
                }
        }
    }

    public void FullScreenUpdate(bool fullscreen)
    {
        if (fullscreen)
        {
            _toggleSprite.sprite = fullScreenOn;
            fullscreenStatus = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            _toggleSprite.sprite = fullScreenOff;
            fullscreenStatus = FullScreenMode.Windowed;
        }
        ChangeResolution();
    }

    public void CloseSettings()
    {
        Debug.Log("closing settings");
        Debug.Log(transform.parent.gameObject);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        if (SceneManager.GetActiveScene().name == "0-1 MainMenu")
        {
            MainMenu.SetActive(true);
        }
        else
        {
            LevelManager.Instance.OpenGameMenu();
            gameObject.SetActive(false);
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("Save deleted.");
        }
        else
        {
            Debug.Log("No save file found to delete.");
        }
    }
}
