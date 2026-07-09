using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SettingMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject topSettings, botSettings, banner, options, optionsBack, optionsPaper;
    [SerializeField]
    private List<GameObject> optionsHitboxes = new List<GameObject>();
    [SerializeField]
    private float bannertweenInLength, tweenInLength;
    public bool isMainMenu;
    [SerializeField]
    private GameObject PauseMenu, SettingsMenu;

    public static SettingMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        ResetAll();
        if (isMainMenu)
        {
            OpenSettings();
        }
        else
        {
            OpenPause();
        }
    }

    private void TweenTopBot()
    {
        topSettings.transform.DOMoveY(transform.position.y + 40f, tweenInLength).SetUpdate(true);
        if (isMainMenu)
        {
            botSettings.transform.DOMoveY(transform.position.y - 40f, tweenInLength).SetUpdate(true).onComplete = TweenOptionsBack;
        }
        else
        {
            botSettings.transform.DOMoveY(transform.position.y - 40f, tweenInLength).SetUpdate(true);
        }
    }

    private void TweenOptionsBack()
    {
        optionsBack.transform.DOLocalMove(new Vector3(-650f, 392f, 0f), tweenInLength).SetUpdate(true);
    }

    private void ResetAll()
    {
        topSettings.transform.DOMoveY(transform.position.y + 500f, 0f).SetUpdate(true);
        botSettings.transform.DOMoveY(transform.position.y - 500f, 0f).SetUpdate(true);
        banner.transform.DOScale(0f, 0f).SetUpdate(true);
        banner.transform.DOLocalMove(new Vector3(0f, 0f, 0f), 0f).SetUpdate(true);
        options.transform.DOScale(0f, 0f).SetUpdate(true);
        optionsBack.transform.DOLocalMove(new Vector3(-1350f, 392f, 0f), 0f).SetUpdate(true);
        optionsPaper.transform.DOLocalMove(new Vector3(0f, 0f, 0f), 0f).SetUpdate(true);
        SetOptionsHitboxes(false);
    }

    private void OpenSettings()
    {
        SetOptionsHitboxes(true);
        options.transform.DOScale(1f, bannertweenInLength).SetUpdate(true).onComplete = TweenTopBot;
    }

    private void OpenPause()
    {
        banner.transform.DOScale(1f, bannertweenInLength).SetUpdate(true).onComplete = TweenTopBot;
    }

    public void ToggleFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void BackButton()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
    }

    public void SettingsButton()
    {
        /*
        banner.transform.DOLocalMoveX(-1800f, bannertweenInLength).SetUpdate(true);
        options.transform.DOLocalMoveX(1800f, 0f).SetUpdate(true);
        options.transform.DOScale(1f, 0f).SetUpdate(true);
        options.transform.DOLocalMoveX(0f, bannertweenInLength).SetUpdate(true);
        TweenOptionsBack();
        SetOptionsHitboxes(true);*/
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(true);
    }

    public void QuitButton()
    {
        Time.timeScale = 1f;
        LevelManager.Instance.LoadScene("0-1 MainMenu");
        PauseMenu.SetActive(false);
    }

    public void CheckpointButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PauseMenu.SetActive(false);
    }

    private void SetOptionsHitboxes(bool setting)
    {
        foreach(GameObject currHitbox in optionsHitboxes)
        {
            currHitbox.SetActive(setting);
        }
    }

    public void OptionsCloseButton()
    {
        if (isMainMenu)
        {
            gameObject.SetActive(false);
        }
        else
        {
            SetOptionsHitboxes(false);
            options.transform.DOLocalMoveX(1800f, bannertweenInLength).SetUpdate(true);
            banner.transform.DOLocalMoveX(0f, bannertweenInLength).SetUpdate(true);
            optionsBack.transform.DOLocalMove(new Vector3(-1350f, 392f, 0f), tweenInLength).SetUpdate(true);
        }
    }
}
