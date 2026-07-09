using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class LevelSelectButton
{
    public int SceneIndex;
    public Button Button;
}

public class LevelSelectCheck : MonoBehaviour
{
    public List<LevelSelectButton> LevelSelectButtons;

    private int _maxLvlIndex = -1;

    void Start()
    {
        if (!SaveManager.Instance)
        {
            Debug.LogError("LevelSelect :: No SaveManager found");
            return;
        }

        _maxLvlIndex = SaveManager.Instance.LoadGame().maxLevel;
        GrayOutUI();
    }

    private void GrayOutUI()
    {
        foreach (var lsb in LevelSelectButtons)
        {
            lsb.Button.interactable = lsb.SceneIndex <= _maxLvlIndex;
            lsb.Button.gameObject.GetComponent<EventTrigger>().enabled = lsb.SceneIndex <= _maxLvlIndex;
        }
    }
}
