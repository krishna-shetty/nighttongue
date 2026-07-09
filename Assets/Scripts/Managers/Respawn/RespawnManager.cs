using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    private GameObject _player;
    private Vector3 _playerStartPos;
    private List<RespawnPoint> _respawnPoints; // for any future checks
    private RespawnPoint _latestRespawn;
    private CharacterController _cc;

    public static RespawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Initialize();
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public void InitializePlayer()
    {
        if (_player) return;
        _player = GameObject.FindGameObjectWithTag("Player");

        if (!_player)
        {
            Debug.Log("RespawnManager :: Could not find player object.");
        }
        else
        {
            Debug.Log("RespawnManager :: Successfully initialized player.");
            _cc = _player.GetComponent<CharacterController>();
            _playerStartPos = _player.transform.position;
        }

        if (!_cc)
        {
            Debug.Log("RespawnManager :: Player object does not have a CharacterController component.");
        }
    }

    private void Initialize()
    {
        InitializePlayer();
        _respawnPoints = new();
        _latestRespawn = null;
    }

    public void RespawnPlayerAtLastPoint()
    {
        if (!_player)
        {
            return;
        }
        
        if (_cc) _cc.enabled = false;

        Vector3 respawnPos;

        if (!_latestRespawn)
        {
            Debug.Log("RespawnManager :: Latest respawn not found, setting position to " + _playerStartPos);
            respawnPos = _playerStartPos;
        }
        else
        {
            var custom = _latestRespawn.customRespawnLocation;
            respawnPos = custom ? custom.position : _latestRespawn.transform.position;
        }

        _player.GetComponent<PlayerController>().UnregisterAllForceSources();
        _player.transform.position = respawnPos;

        var controller = _player.GetComponent<PlayerController>();
        controller.Velocity = Vector2.zero;
        controller.TransitionToState(new FallingState(controller, controller.MoveAction, controller.JumpAction));

        if (_cc) _cc.enabled = true;
    }

    public void SetLatestRespawn(RespawnPoint p)
    {
        if (_respawnPoints.Contains(p)) _respawnPoints.Add(p);
        _latestRespawn = p;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoad;
        Instance = null;
    }
}
