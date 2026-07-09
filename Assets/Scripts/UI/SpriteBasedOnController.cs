using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class SpriteBasedOnController : MonoBehaviour
{
    public Sprite KeyboardSprite;
    public Sprite PlaystationSprite;
    public Sprite XboxControllerSprite;

    private SpriteRenderer _sr;
    private bool _usingKeyboard = true;

    private InputManager _inputManager;

    private void OnEnable()
    {
        _sr = GetComponent<SpriteRenderer>();
        _inputManager = InputManager.Instance;
        _inputManager.OnInputDeviceChanged += UpdateDevice;
    }

    private void OnDisable()
    {
        if (_inputManager != null)
            _inputManager.OnInputDeviceChanged -= UpdateDevice;
    }

    private void UpdateDevice(InputDevice device)
    {
        var deviceIsKeyboard = device is Keyboard || device is Mouse;
        var deviceIsDualShock = device is DualShockGamepad;
        if (_usingKeyboard == deviceIsKeyboard) return;
        _usingKeyboard = deviceIsKeyboard;
        if (_usingKeyboard)
        {
            _sr.sprite = KeyboardSprite;
        }
        else
        {
            if (deviceIsDualShock)
            {
                _sr.sprite = PlaystationSprite;
            }
            else
            {
                _sr.sprite = XboxControllerSprite;
            }
        }
    }
}
