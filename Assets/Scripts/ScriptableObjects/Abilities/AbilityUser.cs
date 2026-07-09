using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Collections;

public class AbilityUser : MonoBehaviour
{
    [FormerlySerializedAs("inputActions")]
    public InputActionAsset InputActions;

    [FormerlySerializedAs("abilities")]
    public List<AbilitySO> Abilities;

    public event Action<AbilitySO> OnAbilityActivated;
    public event Action<AbilitySO> OnAbilityCanceled;

    private GameObject Tongue;
    private PlayerController _controller;
    private AbilitySO _activeAbility;
    private InputAction _activeAction;
    private AbilityHandlerBase _activeHandler;
    private AbilityHandlerBase[] _handlers;
    private bool _isFrozen = false;

    // Store results of latest hold/toggle 
    private AbilityStartResult? _holdResult;
    private AbilityStartResult? _toggleResult;

    // Keep delegates in a dictionary
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _pressDelegates = new();
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _holdDelegates = new();
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _releaseDelegates = new();

    private static Dictionary<Type, AbilityHandlerBase> _handlerMap = new();
    private Dictionary<Type, Type> _handlerTypeMap = new()
    {
        { typeof(GrappleHandler), typeof(GrappleAbilitySO) },
        { typeof(SwingHandler), typeof(SwingAbilitySO) },
        { typeof(DragHandler) , typeof(DragAbilitySO) },
        { typeof(TongueTransformHandler) , typeof(TongueTransformSO) },
        { typeof(UniversalTongueHandler), typeof(UniversalTongueAbilitySO) },
    };

    public T GetAbility<T>() where T : AbilitySO
    {
        foreach (var ability in Abilities)
        {
            if (ability is T typedAbility)
                return typedAbility;
        }

        return null;
    }

    private void Awake()
    {
        // initialize handlers
        Tongue = transform.Find("Tongue")?.gameObject;
        _controller = GetComponent<PlayerController>();
        var inputHandler = GetComponent<PlayerInputHandler>();
        var tongueController = Tongue ? Tongue.GetComponent<TongueController>() : null;

        if (!_controller) Debug.LogError("AbilityUser :: PlayerController component not found on the GameObject.");
        if (!inputHandler) Debug.LogError("AbilityUser :: PlayerInputHandler component not found on the GameObject.");
        if (!Tongue) Debug.LogError("AbilityUser :: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        if (!tongueController) Debug.LogError("AbilityUser :: TongueController component not found on the Tongue GameObject.");

        _handlers = GetComponents<AbilityHandlerBase>();
        foreach (var handler in _handlers)
        {
            Debug.Log("ALL HANDLERS: "+ handler);
            handler.Initialize(this, _controller, inputHandler, Tongue, tongueController, Abilities);
            Type abilitySOType = _handlerTypeMap[handler.GetType()];
            _handlerMap[abilitySOType] = handler;
        }
    }

    private void OnEnable()
    {
        foreach (var ability in Abilities) AddAbilityDelegates(ability);
        AbilityCooldownManager.InitializeAbilityAvailableAt(Abilities);
    }

    private void OnDisable()
    {
        foreach (var ability in Abilities) RemoveAbilityDelegates(ability);
        _pressDelegates.Clear();
        _holdDelegates.Clear();
        _releaseDelegates.Clear();
    }

    private void Update()
    {
        if (_activeHandler) _activeHandler.Tick();

        if (_handlers != null)
        {
            for (int i = 0; i < _handlers.Length; i++)
            {
                var h = _handlers[i];
                if (h == null || ReferenceEquals(h, _activeHandler)) continue;
                if (h.NeedsTick) h.Tick();
            }
        }
    }

    public void ActivateAbility(AbilitySO ability)
    {
        if (!ability || !_handlerMap.ContainsKey(ability.GetType()))
        {
            Debug.LogWarning("AbilityUser::ActivateAbility : returning");
            return;
        }

        if (!AbilityCooldownManager.IsAvailable(ability))
            AbilityCooldownManager.AddAbility(ability);

        OnButtonPressed(ability);
    }

    /// <summary>
    /// Any non-passive abilities should never be activated outside of this button press delegate
    /// </summary>
    private void OnButtonPressed(AbilitySO ability)
    {
        if (_isFrozen || !ability || !AbilityCooldownManager.IsAvailable(ability)) return;

        if (!_handlerMap.ContainsKey(ability.GetType()))
        {
            Debug.LogError("AbilityUser :: AbilitySO " + ability + " is not a type mapped to any handler.");
            return;
        }

        if (_controller.GetCurrentState() is SwingingState) return;

        if (_activeAbility is TongueTransformSO && _handlerMap[ability.GetType()] is not TongueTransformHandler)
        {
            Debug.Log("Cannot grapple while as ball");
            return;
        }
            

        // Release active ability if different from new ability
        if (_activeAbility != null && _activeAbility != ability)
            ReleaseActiveHandler(_activeAbility);

        // Invoke
        var handler = _handlerMap[ability.GetType()];
        _holdResult = handler.Hold(ability);
        _toggleResult = handler.Toggle(ability);

        if (_holdResult?.ActivationSuccessful is true || _toggleResult?.ActivationSuccessful is true)
        {
            //Debug.Log("ACTIVATING " + ability);
            _activeAbility = ability;
            _activeHandler = handler;
            OnAbilityActivated?.Invoke(_activeAbility);
        }
    }

    private void OnButtonReleased(AbilitySO ability)
    {
        if (!ability || !_handlerMap.ContainsKey(ability.GetType()))
        {
            Debug.Log("AbilityUser :: AbilitySO released not mapped to any handler.");
            return;
        }

        // release if last ability activated used a hold implementation
        if (_holdResult?.ActivationSuccessful is true)
        {
            //Debug.Log("RELEASING HOLD " + ability);
            //_handlerMap[ability.GetType()].Release();
            ReleaseActiveHandler(ability);
        }
    }

    public void RequestMovementStateChange(MovementStateIntent intent)
    {
        if (_activeAbility == intent.ActiveAbility ||
            _activeAbility == intent.OwnerAbility)
            _controller.HandleMovementStateRequest(intent);
    }

    /// <summary>
    /// Releases active handler if activeAbility == ability. Returns _activeHandler == null (indicating successful release)
    /// </summary>
    public bool ReleaseActiveHandler(AbilitySO ability)
    {
        if (_activeHandler && _activeAbility && _activeAbility == ability)
        {
            //Debug.Log("RELEASING ACTIVE HANDLER FOR " + ability);
            _activeHandler.Release();
        }

        return _activeHandler == null;
    }

    /// <summary>
    /// All AbilityHandlers MUST call this at the end of Release()
    /// </summary>
    public void InvokeOnAbilityCancel(AbilitySO ability)
    {
        if (!ability || !_activeAbility) return;
        OnAbilityCanceled?.Invoke(ability);

        if (ability == _activeAbility)
        {
            //Debug.Log("NULLING FLAGS ON ABILITY CANCEL");
            _activeHandler = null;
            _activeAbility = null;
            _holdResult = null;
            _toggleResult = null;
        }
    }

    public void FreezeAbilities(float duration)
    {
        _isFrozen = true;
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _isFrozen = false;
    }

    public void MapHandler(AbilitySO ability, AbilityHandlerBase handler)
    {
        ability.GetType();
        _handlerMap[ability.GetType()] = handler;
    }

    public void AddAbilitySO(AbilitySO ability)
    {
        if (Abilities.Contains(ability)) return;

        Abilities.Add(ability);
        AddAbilityDelegates(ability);
        AbilityCooldownManager.AddAbility(ability);
    }

    public void RemoveAbilitySO(AbilitySO ability)
    {
        if (!Abilities.Contains(ability)) return;

        ReleaseActiveHandler(ability);
        RemoveAbilityDelegates(ability);
        AbilityCooldownManager.RemoveAbility(ability);
        Abilities.Remove(ability);
    }

    private void AddAbilityDelegates(AbilitySO ability)
    {
        var action = InputActions.FindAction(ability.InputActionName, true);
        if (action == null) return;

        // Prepare delegates
        _pressDelegates[ability] = ctx => OnButtonPressed(ability);
        _holdDelegates[ability] = ctx => ability.OnHold(gameObject);
        if (ability.IsHoldAbility)
            _releaseDelegates[ability] = ctx => OnButtonReleased(ability);

        // Subscribe
        action.started += _pressDelegates[ability];
        action.performed += _holdDelegates[ability];
        if (_releaseDelegates.ContainsKey(ability)) action.canceled += _releaseDelegates[ability];
        action.Enable();
    }

    private void RemoveAbilityDelegates(AbilitySO ability)
    {
        var action = InputActions.FindAction(ability.InputActionName, true);
        if (action == null) return;

        if (_pressDelegates.TryGetValue(ability, out var press)) action.started -= press;
        if (_holdDelegates.TryGetValue(ability, out var hold)) action.performed -= hold;
        if (_releaseDelegates.TryGetValue(ability, out var release)) action.canceled -= release;

        action.Disable();
    }

    public AbilitySO GetActiveAbility() { return _activeAbility; }

    public void SetFrozen(bool frozen)
    {
        _isFrozen = frozen;

        // Releasing active handler on freeze caused a game crashing bug on swing release, so commenting out for now. 
        // Will need to revisit freeze logic if we want to be able to freeze during swings. 

        // if (frozen)
        // {
        //     ReleaseActiveHandler(_activeAbility);
        // }
    }
}
