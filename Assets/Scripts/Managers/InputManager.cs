using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _controls;

    public Vector2 MoveInput { get; private set; }

    public event Action OnJumpStarted;
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled;
    public event Action OnPickup;
    public event Action OnCancelStarted;
    public event Action OnCancelPerformed;
    public event Action OnCancelCanceled;
    public event Action<InputDevice> OnInputDeviceChanged;
    public InputDevice LastUsedDevice { get; private set; }

    // Store delegates
    //Player
    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled;
    private Action<InputAction.CallbackContext> _onJumpStarted;
    private Action<InputAction.CallbackContext> _onJumpPerformed;
    private Action<InputAction.CallbackContext> _onJumpCanceled;
    private Action<InputAction.CallbackContext> _onPickupPerformed;


    //UI
    private Action<InputAction.CallbackContext> _onCancelStarted;
    private Action<InputAction.CallbackContext> _onCancelPerformed;
    private Action<InputAction.CallbackContext> _onCancelCanceled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _controls = new InputSystem_Actions();

        InputSystem.onActionChange += OnActionChange;

        // Create delegates
        _onMovePerformed = ctx => MoveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => MoveInput = Vector2.zero;

        _onJumpStarted = ctx => OnJumpStarted?.Invoke();
        _onJumpPerformed = ctx => OnJumpPerformed?.Invoke();
        _onJumpCanceled = ctx => OnJumpCanceled?.Invoke();
        _onPickupPerformed = ctx => OnPickup?.Invoke();

        _onCancelStarted = ctx => OnCancelStarted?.Invoke();
        _onCancelPerformed = ctx => OnCancelPerformed?.Invoke();
        _onCancelCanceled = ctx => OnCancelCanceled?.Invoke();
    }
    
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;
        if (obj is InputAction action)
        {
            LastUsedDevice = action.activeControl?.device;
            OnInputDeviceChanged?.Invoke(LastUsedDevice);
        }
    }

    private void OnEnable()
    {
        // Only the real singleton should wire things up
        if (Instance != this) return;

        // Lazy-create the Input Actions
        if (_controls == null)
            _controls = new InputSystem_Actions();

        // Create delegates once
        if (_onMovePerformed == null)
        {
            _onMovePerformed = ctx => MoveInput = ctx.ReadValue<Vector2>();
            _onMoveCanceled = ctx => MoveInput = Vector2.zero;

            _onJumpStarted = ctx => OnJumpStarted?.Invoke();
            _onJumpPerformed = ctx => OnJumpPerformed?.Invoke();
            _onJumpCanceled = ctx => OnJumpCanceled?.Invoke();
        }

        // Subscribe
        var player = _controls.Player;
        player.Move.performed += _onMovePerformed;
        player.Move.canceled += _onMoveCanceled;

        player.Jump.started += _onJumpStarted;
        player.Jump.performed += _onJumpPerformed;
        player.Jump.canceled += _onJumpCanceled;

        player.Pickup.performed += _onPickupPerformed;

        var ui = _controls.UI;

        ui.Cancel.started += _onCancelStarted;
        ui.Cancel.performed += _onCancelPerformed;
        ui.Cancel.canceled += _onCancelCanceled;

        _controls.Enable();
    }

    private void OnDisable()
    {
        if (Instance != this || _controls == null) return;
        // Unsubscribe
        var player = _controls.Player;
        player.Move.performed -= _onMovePerformed;
        player.Move.canceled -= _onMoveCanceled;

        player.Jump.started -= _onJumpStarted;
        player.Jump.performed -= _onJumpPerformed;
        player.Jump.canceled -= _onJumpCanceled;

        player.Pickup.performed -= _onPickupPerformed;


        var ui = _controls.UI;

        ui.Cancel.started -= _onCancelStarted;
        ui.Cancel.performed -= _onCancelPerformed;
        ui.Cancel.canceled -= _onCancelCanceled;

        _controls.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
