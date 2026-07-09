using System.Collections.Generic;
using UnityEngine;
using System;

public class UniversalTongueHandler : AbilityHandlerBase
{
    private SwingAbilitySO _swingAbility;
    private GrappleAbilitySO _grappleAbility;

    private SwingHandler _swingHandler;
    private GrappleHandler _grappleHandler;

    private AbilityHandlerBase _activeSubHandler;
    private AbilitySO _activeSubAbility;
    public event Action FailedAnimationEvent;

    public override void Initialize(AbilityUser user, PlayerController controller, PlayerInputHandler inputHandler, GameObject tongue, TongueController tongueController, List<AbilitySO> abilities = null)
    {
        base.Initialize(user, controller, inputHandler, tongue, tongueController, abilities);

        Debug.Log("UniversalTongueHandler: Initializing sub-handlers");
        _swingHandler = user.GetComponent<SwingHandler>();
        _grappleHandler = user.GetComponent<GrappleHandler>();

        if (!_swingHandler) Debug.LogError("Universal: No SwingHandler found!");
        if (!_grappleHandler) Debug.LogError("Universal: No GrappleHandler found!");
    }

    public override AbilityStartResult? Toggle(AbilitySO ability)
    {
        if (ability is not UniversalTongueAbilitySO universalTongueAbility)
        {
            Debug.LogError("UniversalTongueHandler: Hold must have UniversalTongueAbilitySO passed in");
            return null;
        }

        base.Toggle(ability);

        Debug.Log("UniversalTongueHandler: Toggle called");

        _swingAbility = universalTongueAbility.SwingAbility;
        _grappleAbility = universalTongueAbility.GrapplingAbility;

        AbilityCooldownManager.AddAbility(_swingAbility);
        AbilityCooldownManager.AddAbility(_grappleAbility);

        if (_swingHandler && _swingHandler.PreCheck(_swingAbility).CanActivate)
        {
            Debug.Log("UniversalTongueHandler: Activating Swing Ability");
            ActivateSubHandler(_swingHandler, _swingAbility);
            return new AbilityStartResult(true);
        }

        else if (_grappleHandler && _grappleHandler.PreCheck(_grappleAbility).CanActivate)
        {
            Debug.Log("UniversalTongueHandler: Activating Grapple Ability");
            ActivateSubHandler(_grappleHandler, _grappleAbility);
            return new AbilityStartResult(true);
        }
        FailedAnimationEvent?.Invoke();
        //Debug.Log("Called FAIL EVENT");

        return null;
    }
   
    private void ActivateSubHandler(AbilityHandlerBase handler, AbilitySO ability)
    {
        _activeSubHandler = handler;
        _activeSubAbility = ability;

        handler.Toggle(ability);
    }

    public override void Release()
    {
        if (_activeSubHandler != null)
        {
            _activeSubHandler.Release();
            _activeSubHandler = null;
            _activeSubAbility = null;
        }

        // Release universal as the "global" active ability
        base.Release();
    }
}
