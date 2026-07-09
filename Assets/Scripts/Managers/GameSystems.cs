using UnityEngine;

public class GameSystems : MonoBehaviour
{
    public static GameSystems Instance { get; private set; }
    
    [SerializeField] private GameObject wwiseGlobalPrefab;

    private void Awake()
    {
        // Ensure only one instance of GameSystems exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        
        LoadWwiseGlobalSafely();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LoadWwiseGlobalSafely()
    {
        var _wwiseGlobal = FindAnyObjectByType<SoundtrackManager>();
        if (_wwiseGlobal == null)
        {
            Instantiate(wwiseGlobalPrefab);
        }
    }
}
