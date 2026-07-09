using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SwapBartenderDialogue : MonoBehaviour
{
    public string heartYarnNode = "";
    public int heartLvBuildIndex = 4;

    private readonly string _thirdNode = "thirdBartender";

    private bool _showingHeartDialogue = false;
    private bool _showingLastDialogue = false;
    private SaveFileData _save;

    public UnityEvent SkipToLevelSelect;

    // no i will not be refactoring these shitty hard coded checks or the savemanager stuff with 1 week remaining in the semester
    void Start()
    {
        var saveManager = SaveManager.Instance;
        var dialogueScript = GetComponent<TalkWhenCloseToTami>();
        if (!saveManager || !dialogueScript) Debug.LogError("Could not change bartender dialogue (no SaveManager or TalkWhenCloseToTami)");

        _save = saveManager.LoadGame();
        if (_save.maxLevel >= heartLvBuildIndex)
        {
            if (_save.showLastBarCutsceneNext) // bartender dialogue #3
            {
                _showingLastDialogue = true;
                dialogueScript.YarnNode = _thirdNode;
            }
            else if (!_save.barCutscene) // bartender dialogue #2
            {
                _showingHeartDialogue = true;
                dialogueScript.YarnNode = heartYarnNode;
            }
            else // if the player has seen #2 but hasn't unlocked #3, skip to level select
            {
                var collider = GetComponent<BoxCollider>();
                if (collider) collider.enabled = false;
                StartCoroutine(NextFrame());
            }
        }
    }

    private IEnumerator NextFrame()
    {
        yield return null;
        SkipToLevelSelect.Invoke();
    }

    public void OnDialogueComplete()
    {
        if (_showingLastDialogue)
        {
            GameObject.Find("LevelManager").GetComponent<LevelManager>().playEndingInsteadOfIntro = true;
            SceneManager.LoadScene("0-1 MainMenu");
            return;
        }
        else if (_showingHeartDialogue) SaveManager.Instance.SaveBarCutscene(true);
        SkipToLevelSelect.Invoke();
    }
}