using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField]
    private GameObject _settingsMenu;
    private GameObject _mainMenu;
    private bool _canPause = true;
    [SerializeField]
    private GameObject _settings, _oldPause;
    public bool playedIntro = false;
    public bool playEndingInsteadOfIntro = false;

    private AsyncOperation _preloadedSceneOp = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelPerformed += HandlePauseRequested;
            InputManager.Instance.OnCancelPerformed += OpenMenu;
        }
        
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancelPerformed -= HandlePauseRequested;
            InputManager.Instance.OnCancelPerformed -= OpenMenu;
        }
        
    }

    private void HandlePauseRequested()
    {
        if (!_canPause) return;

        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            OpenGameMenu();
        }
    }

    void Start()
    {
        _settingsMenu = FindObjectOfType<SettingMenu>(true).gameObject;
        _settings = FindObjectOfType<SettingsMenu>(true).gameObject;
        _oldPause = FindObjectOfType<OldPause>(true).gameObject;
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void SetCanPause(bool canPause)
    {
        _canPause = canPause;
    }

    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadSceneAsync(nextIndex);
            LoadingScript loading = FindObjectOfType<LoadingScript>(true);
            loading.gameObject.transform.parent.gameObject.SetActive(true);
            Debug.Log(loading.gameObject.transform.parent.gameObject);
        }
        else
        {
            Debug.Log("No more levels! Quitting game.");
            Application.Quit();
        }
    }

    public void LoadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName) != null)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("No scene found called: " + sceneName);
            Application.Quit();
        }
    }

    public void PreloadScene(string sceneName)
    {
        _preloadedSceneOp = SceneManager.LoadSceneAsync(sceneName);
        _preloadedSceneOp.allowSceneActivation = false;
    }

    public void LoadPreloadedScene()
    {
        _preloadedSceneOp.allowSceneActivation = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        _settingsMenu = FindObjectOfType<SettingMenu>(true).gameObject;
        _settings = FindObjectOfType<SettingsMenu>(true).gameObject;
        _oldPause = FindObjectOfType<OldPause>(true).gameObject;
        _mainMenu = FindObjectOfType<OldPause>(true).gameObject;
        if (_settingsMenu == null)
        {
            Debug.LogError("LevelManager :: No SettingMenu found in the scene. Please ensure there is a GameObject with a SettingMenu component.");
            return;
        }
        _settingsMenu.GetComponent<SettingMenu>().isMainMenu = true;
        GameObject MainMenu = GameObject.Find("Main Menu");
        FindObjectOfType<SettingsMenu>(true).MainMenu = MainMenu;
        MainMenu.SetActive(false);
        _oldPause.SetActive(false);
        _settingsMenu.SetActive(true);
        _settings.SetActive(true);
    }

    public void OpenGameMenu()
    {
        if (_settingsMenu == null)
        {
            Debug.LogError("LevelManager :: No SettingMenu found in the scene. Please ensure there is a GameObject with a SettingMenu component.");
            return;
        }

        if (!_canPause)
        {
            Debug.Log("Can't pause bozo");
            return;
        }
        Debug.Log("Opening Game Menu");
        _settingsMenu.GetComponent<SettingMenu>().isMainMenu = false;
        _settings.SetActive(false);
        _settingsMenu.SetActive(true);
        _oldPause.SetActive(true);
        
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }

        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if(SceneManager.GetActiveScene().name != "Main_Menu")
        //     {
        //         OpenGameMenu();
        //         Time.timeScale = 0f;
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.N)) 
        {
            LoadNextLevel();
        }
    }

    private void OpenMenu()
    {
        if (SceneManager.GetActiveScene().name != "0-1 MainMenu"
            && SceneManager.GetActiveScene().name != "Credits")
        {
            OpenGameMenu();
        }
    }

}
