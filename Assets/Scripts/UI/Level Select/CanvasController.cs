using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelected;
    private GameObject _lastSelected;

    private PlayerInputHandler _inputHandler;

    public static CanvasController Active { get; private set; }

    public void SetLastSelected(GameObject obj) => _lastSelected = obj;

    public void RestoreLastSelected()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            StartCoroutine(SelectNextFrame(_lastSelected ?? defaultSelected));
    }

    private IEnumerator SelectNextFrame(GameObject target)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryFindInputHandler();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindInputHandler();
    }

    private void TryFindInputHandler()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        _inputHandler = player.GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        Active = this;
        StartCoroutine(SelectNextFrame());
        if (_inputHandler)
            _inputHandler.SetFrozen(true);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInputDeviceChanged += OnDeviceChanged;
            OnDeviceChanged(InputManager.Instance.LastUsedDevice);
        }
    }

    private void OnDisable()
    {
        if (Active == this) Active = null;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        if (_inputHandler)
            _inputHandler.SetFrozen(false);

        if (InputManager.Instance != null)
            InputManager.Instance.OnInputDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(InputDevice device)
    {
        bool isKeyboard = device is Keyboard || device is Mouse;
        Cursor.lockState = isKeyboard ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = isKeyboard;

        if (!isKeyboard)
            RestoreLastSelected();
    }

    private IEnumerator SelectNextFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(defaultSelected);
        yield return null;
    }

    public void SelectDefault()
    {
        StartCoroutine(SelectNextFrame());
    }
}