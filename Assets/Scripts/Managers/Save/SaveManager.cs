using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class SaveFileData
{
    public int level;
    public int checkpoint;
    public int maxLevel;
    public bool barCutscene;
    public bool showLastBarCutsceneNext;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    [Serializable]
    public class SaveData
    {
        public Vector3 checkpointPosition;
        public List<GameObject> checkpointItems;
        public List<Vector3> checkpointItemsPosition;
        public SaveData(Vector3 checkpoint, List<GameObject> checkpointItems, List<Vector3> checkpointItemsPosition)
        {
            this.checkpointPosition = checkpoint;
            this.checkpointItems = checkpointItems;
            this.checkpointItemsPosition = checkpointItemsPosition;
        }
    }

    [SerializeField] public int currCheckpoint = 0;
    private String sceneName = "";

    [Serializable]
    public class SaveDataItem
    {
        [SerializeField]
        public String name;
        [SerializeField]
        public SaveData[] saveData;
    }

    [SerializeField]
    private SaveDataItem[] saveData;
    private Dictionary<String, SaveData[]> saveDataMap;

    private SaveFileData _freeSave;

    private Dictionary<String, SaveData[]> ToSaveDataMap(SaveDataItem[] saveDataMap)
    {
        Dictionary<String, SaveData[]> map = new Dictionary<String, SaveData[]>();
        foreach (SaveDataItem item in saveDataMap)
        {
            map[item.name] = item.saveData;
        }
        return map;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sceneName = SceneManager.GetActiveScene().name;

        saveDataMap = ToSaveDataMap(saveData);

        saveDataMap = ToSaveDataMap(saveData);
        _freeSave = new SaveFileData();
        if (!saveDataMap.TryGetValue(sceneName, out var checkpoints) || checkpoints == null)
        {
            Debug.Log($"SaveManager: No save data found for scene '{sceneName}'. Check SaveDataItem.name in inspector.");
            return;
        }
        foreach (var currSave in checkpoints)
        {
            if (currSave == null) continue;

            if (currSave.checkpointItems == null) currSave.checkpointItems = new List<GameObject>();
            if (currSave.checkpointItemsPosition == null) currSave.checkpointItemsPosition = new List<Vector3>();

            if (currSave.checkpointItems.Count != currSave.checkpointItemsPosition.Count)
                Debug.Log($"SaveManager: Scene '{sceneName}' checkpoint mismatch items vs positions.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != sceneName)
        {
            sceneName = scene.name;
            currCheckpoint = 0;
        }
        SetPlayerToCheckPoint();
        SaveGame(0);
    }

    public void SetPlayerToCheckPoint()
    {
        GameObject Player = GameObject.FindWithTag("Player");
        if (Player != null)
        {
            Player.transform.position = saveDataMap[sceneName][currCheckpoint].checkpointPosition;
            for (int i = 0; i < saveDataMap[sceneName][currCheckpoint].checkpointItems.Count; i++)
            {
                Instantiate(saveDataMap[sceneName][currCheckpoint].checkpointItems[i], saveDataMap[sceneName][currCheckpoint].checkpointItemsPosition[i], Quaternion.identity);
            }
        }
    }

    public void SetCurrentCheckpoint(int checkpoint) { currCheckpoint = checkpoint; }

    public void SaveMaxLevel(int level)
    {
        SaveFileData pastSave = LoadGame();
        pastSave.maxLevel = level;
        SaveGame(pastSave.checkpoint, pastSave);
    }

    public void SaveBarCutscene(bool cutscene)
    {
        SaveFileData pastSave = LoadGame();
        pastSave.barCutscene = cutscene;
        SaveGame(pastSave.checkpoint, pastSave);
    }

    public void SaveLastBarCutscene(bool cutscene)
    {
        SaveFileData pastSave = LoadGame();
        pastSave.showLastBarCutsceneNext = cutscene;
        LevelManager.Instance.playedIntro = false;
        SaveGame(pastSave.checkpoint, pastSave);
    }

    public void SaveGame(int checkpoint, SaveFileData save = null)
    {
        SaveFileData pastSave = save ?? LoadGame();
        if (pastSave == null)
        {
            _freeSave.level = 3;
            _freeSave.maxLevel = 3;
            _freeSave.checkpoint = 0;
            _freeSave.barCutscene = false;
            _freeSave.showLastBarCutsceneNext = false;
            string jsonsave = JsonUtility.ToJson(_freeSave);
            File.WriteAllText(Application.persistentDataPath + "/save.json", jsonsave);
            return;
        }
        _freeSave.level = SceneManager.GetActiveScene().buildIndex;
        if (_freeSave.level < 3)
        {
            _freeSave.level = 3;
        }
        if (_freeSave.maxLevel == null || pastSave.maxLevel < _freeSave.level)
        {
            _freeSave.maxLevel = _freeSave.level;
        }
        else
        {
            _freeSave.maxLevel = pastSave.maxLevel;
        }
        _freeSave.checkpoint = checkpoint;
        _freeSave.barCutscene = pastSave.barCutscene;
        _freeSave.showLastBarCutsceneNext = pastSave.showLastBarCutsceneNext;
        string json = JsonUtility.ToJson(_freeSave);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public SaveFileData LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveFileData>(json);
        }
        return null;
    }
}
