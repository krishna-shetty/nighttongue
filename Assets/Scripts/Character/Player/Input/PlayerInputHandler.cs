using UnityEngine;
using System;
using System.Collections;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action OnJumpRequested;
    public event Action OnJumpHeld;
    public event Action OnJumpReleased;
    public event Action OnPickup;

    [Header("Input Properties")]
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpHeld { get; private set; }

    private bool _isFrozen = false;
    private bool _isSubscribed = false;
    private AbilityUser _abilityUser;

    private void Awake()
    {
        _abilityUser = GetComponent<AbilityUser>();
        if (_abilityUser == null)
        {
            Debug.LogWarning("PlayerInputHandler :: No AbilityUser found on this GameObject.");
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;
        if (InputManager.Instance == null) return;

        InputManager.Instance.OnJumpStarted += HandleJumpStarted;
        InputManager.Instance.OnJumpPerformed += HandleJumpPerformed;
        InputManager.Instance.OnJumpCanceled += HandleJumpCanceled;
        InputManager.Instance.OnPickup += HandlePickup;

        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;
        if (InputManager.Instance == null) return;

        InputManager.Instance.OnJumpStarted -= HandleJumpStarted;
        InputManager.Instance.OnJumpPerformed -= HandleJumpPerformed;
        InputManager.Instance.OnJumpCanceled -= HandleJumpCanceled;
        InputManager.Instance.OnPickup -= HandlePickup;

        _isSubscribed = false;
    }

    private void Update()
    {
        if (!_isSubscribed)
            TrySubscribe();

        if (InputManager.Instance != null)
        {
            MoveInput = _isFrozen ? Vector2.zero : InputManager.Instance.MoveInput;
        }
    }

    private void ProcessInputWrapper(Action stateUpdate, Action inputEvent)
    {
        if (_isFrozen) return;

        stateUpdate?.Invoke();
        inputEvent?.Invoke();
    }

    private void HandleJumpStarted()
    {
        ProcessInputWrapper(() => IsJumpHeld = true, OnJumpRequested);
    }

    private void HandleJumpPerformed()
    {
        ProcessInputWrapper(() => IsJumpHeld = true, OnJumpHeld);
    }

    private void HandleJumpCanceled()
    {
        ProcessInputWrapper(() => IsJumpHeld = false, OnJumpReleased);
    }

    private void HandlePickup()
    {
        if (_isFrozen) return;
        OnPickup?.Invoke();
    }

    public void SetFrozen(bool frozen)
    {
        _isFrozen = frozen;

        if (frozen)
        {
            MoveInput = Vector2.zero;
            IsJumpHeld = false;
        }

        if (_abilityUser != null)
            _abilityUser.SetFrozen(frozen);
    }

    public void FreezeInput(float duration)
    {
        SetFrozen(true);
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetFrozen(false);
    }
}