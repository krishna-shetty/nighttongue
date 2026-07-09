using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class FlavorManager : MonoBehaviour
{
    [SerializeField]
    private FlavorSO _activeActiveFlavorSO;
    private IFlavorRuntime _activeFlavorRuntime = null;

    [SerializeField]
    private FlavorSO _activePassiveFlavorSO;
    private IFlavorRuntime _passiveFlavorRuntime = null;

    private FlavorRuntimeFactory _factory;
    private ComboFactory _comboFactory;
    private UIContextProvider _contextProvider;

    [SerializeField]
    private BitterSweetComboSO _bitterSweetCombo = null;
    [SerializeField]
    private SweetBitterComboSO _sweetBitterCombo = null;
    [SerializeField]
    private BitterSaltyComboSO _bitterSaltyCombo = null;

    private Dictionary<FlavorSO, List<AbilitySO>> _grantedAbilitiesMap;

    private Dictionary<Tuple<EFlavor, EFlavor>, ComboSO> _comboDataMap;

    private PlayerInputHandler _inputHandler;
    private AbilityUser _abilityUser;

    public FlavorSO GetActiveActiveFlavor() => _activeActiveFlavorSO;
    public FlavorSO GetActivePassiveFlavor() => _activePassiveFlavorSO;

    public event Action<FlavorSO> OnPickupFlavor;

    private void InitializeComboHandlerMap()
    {
        _comboDataMap = new Dictionary<Tuple<EFlavor, EFlavor>, ComboSO>
        {
            { new Tuple<EFlavor, EFlavor>(EFlavor.Sweet, EFlavor.Bitter), _sweetBitterCombo },
            { new Tuple<EFlavor, EFlavor>(EFlavor.Bitter, EFlavor.Sweet), _bitterSweetCombo },
            { new Tuple<EFlavor, EFlavor>(EFlavor.Bitter, EFlavor.Salty), _bitterSaltyCombo },
            { new Tuple<EFlavor, EFlavor>(EFlavor.Salty, EFlavor.Bitter), _bitterSaltyCombo },
        };
    }

    private void Awake()
    {
        _abilityUser = GetComponent<AbilityUser>();
        if (!_abilityUser)
            Debug.LogError("FlavorManager requires an AbilityUser component on the same GameObject.");

        _inputHandler = GetComponent<PlayerInputHandler>();
        if (!_inputHandler)
            Debug.LogError("FlavorManager requires a PlayerInputHandler component on the same GameObject.");

        _grantedAbilitiesMap = new Dictionary<FlavorSO, List<AbilitySO>>();

        _contextProvider = UIContextProvider.Instance ? UIContextProvider.Instance : null;

        _factory = new FlavorRuntimeFactory(this, _contextProvider);
        _comboFactory = new ComboFactory(this);
    }

    private void Start()
    {
        InitializeComboHandlerMap();
    }

    private void OnEnable()
    {
        if (_abilityUser)
        {
            _abilityUser.OnAbilityActivated += HandleAbilityActivated;
            _abilityUser.OnAbilityCanceled += HandleAbilityCanceled;
        }
    }

    private void OnDisable()
    {
        if (_abilityUser)
        {
            _abilityUser.OnAbilityActivated -= HandleAbilityActivated;
            _abilityUser.OnAbilityCanceled -= HandleAbilityCanceled;
        }
    }

    private void Update()
    {
        _activeFlavorRuntime?.Tick(Time.deltaTime);
        _passiveFlavorRuntime?.Tick(Time.deltaTime);
    }

    private ComboHandlerBase Resolve(EFlavor initialFlavor, EFlavor nextFlavor)
    {
        if (_comboDataMap.TryGetValue(new Tuple<EFlavor, EFlavor>(initialFlavor, nextFlavor), out var comboSO))
        {
            return _comboFactory.CreateComboHandler(comboSO);
        }

        return null;
    }

    private void TryExecuteCombo(EFlavor first, EFlavor second)
    {
        ComboHandlerBase handler = Resolve(first, second);

        if (handler != null)
        {
            StartCoroutine(handler.Execute());
        }
        else
        {
            Debug.Log("FlavorManager::TryExecuteCombo : No combo found for " + first + " + " + second);
        }
    }

    public void OnFlavorPickup(FlavorSO flavor)
    {
        OnPickupFlavor?.Invoke(flavor); // SFX

        if (flavor is ActiveFlavorSO activeFlavor)
        {
            if (_activeFlavorRuntime != null)
            {
                _activeFlavorRuntime.OnExpired -= HandleActiveFlavorExpired;
                _activeFlavorRuntime.OnDeactivate();
                HandleActiveFlavorExpired(_activeFlavorRuntime);
            }

            _activeFlavorRuntime = _factory.CreateRuntime(activeFlavor);
            _activeFlavorRuntime.OnActivate();
            _activeFlavorRuntime.OnExpired += HandleActiveFlavorExpired;
            _activeActiveFlavorSO = activeFlavor;

            GrantAbilitiesFromFlavor(activeFlavor);

            //if (_activeActiveFlavorSO && _activePassiveFlavorSO)
            //    TryExecuteCombo(_activePassiveFlavorSO.Flavor, _activeActiveFlavorSO.Flavor);

        }
        else if (flavor is PassiveFlavorSO passiveFlavor)
        {
            if (_passiveFlavorRuntime != null)
            {
                _passiveFlavorRuntime.OnExpired -= HandlePassiveFlavorExpired;
                _passiveFlavorRuntime.OnDeactivate();
                HandlePassiveFlavorExpired(_passiveFlavorRuntime);
            }

            _passiveFlavorRuntime = _factory.CreateRuntime(passiveFlavor);
            _passiveFlavorRuntime.OnActivate();
            _passiveFlavorRuntime.OnExpired += HandlePassiveFlavorExpired;
            _activePassiveFlavorSO = passiveFlavor;

            GrantAbilitiesFromFlavor(passiveFlavor);

            //if (_activeActiveFlavorSO && _activePassiveFlavorSO)
            //    TryExecuteCombo(_activeActiveFlavorSO.Flavor, _activePassiveFlavorSO.Flavor);
        }
        else
        {
            Debug.LogError("FlavorManager::OnFlavorPickup : The fuck's happening here?");
            return;
        }
    }

    private bool AbilityBelongsToActiveFlavor(AbilitySO ability)
    {
        return _activeActiveFlavorSO &&
               _grantedAbilitiesMap.TryGetValue(_activeActiveFlavorSO, out var list) &&
               list.Contains(ability);
    }

    private void HandleAbilityActivated(AbilitySO ability)
    {
        if (AbilityBelongsToActiveFlavor(ability))
        {
            _activeFlavorRuntime?.OnAbilityActivated();
        }
    }

    private void HandleAbilityCanceled(AbilitySO ability)
    {
        if (AbilityBelongsToActiveFlavor(ability))
            _activeFlavorRuntime?.OnAbilityCanceled();
    }

    private void GrantAbilitiesFromFlavor(FlavorSO flavor)
    {
        _grantedAbilitiesMap[flavor] = new List<AbilitySO>();

        foreach (var ability in flavor.Abilities)
        {
            _grantedAbilitiesMap[flavor].Add(ability);
            _abilityUser.AddAbilitySO(ability);
        }
    }

    private void RemoveAbilitiesFromFlavor(FlavorSO flavor)
    {
        if (!_grantedAbilitiesMap.TryGetValue(flavor, out var abilities))
            return;

        foreach (var ability in abilities)
            _abilityUser.RemoveAbilitySO(ability);

        _grantedAbilitiesMap.Remove(flavor);
    }

    private void HandleActiveFlavorExpired(IFlavorRuntime runtime)
    {
        if (_activeFlavorRuntime == runtime)
        {
            _activeFlavorRuntime.OnExpired -= HandleActiveFlavorExpired;
            _activeFlavorRuntime = null;

            RemoveAbilitiesFromFlavor(_activeActiveFlavorSO);

            _activeActiveFlavorSO = null;
        }
    }

    private void HandlePassiveFlavorExpired(IFlavorRuntime runtime)
    {
        if (_passiveFlavorRuntime == runtime)
        {
            _passiveFlavorRuntime.OnExpired -= HandlePassiveFlavorExpired;
            _passiveFlavorRuntime = null;

            RemoveAbilitiesFromFlavor(_activePassiveFlavorSO);

            _activePassiveFlavorSO = null;
        }
    }
}