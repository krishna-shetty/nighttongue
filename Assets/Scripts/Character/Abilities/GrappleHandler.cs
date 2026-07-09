using System;
using System.Collections.Generic;
using UnityEngine;
using static AbilityHandlerBase;

public class GrappleHandler : TargetAbilityHandlerBase
{
    protected GrappleAbilitySO _activeAbility;
    public LayerMask WhatIsGrappable;
    private Vector3 _grapplePoint;
    private RaycastHit _hit;
    private bool _isGrappling = false;

    public override void Initialize(AbilityUser user, PlayerController controller, PlayerInputHandler inputHandler,
        GameObject tongue, TongueController tongueController, List<AbilitySO> abilities)
    {
        base.Initialize(user, controller, inputHandler, tongue, tongueController);
        _playerController.WhatIsGrappable = WhatIsGrappable;
    }

    public override PreCheckResult PreCheck(AbilitySO ability)
    {
        if (ability is not GrappleAbilitySO grappleAbility)
        {
            Debug.LogError("GrappleHandler: PreCheck must have GrappleAbilitySO passed in");
            return new PreCheckResult(false);
        }

        return DoTargetPreCheck(
            grappleAbility.MaxGrappleDistance,
            grappleAbility.MaxCheckAngle,
            _playerController.WhatIsGrappable
        );
    }

    public override AbilityStartResult? Toggle(AbilitySO ability)
    {
        if (_isGrappling)
        {
            Release();
            return new AbilityStartResult(false);
        }

        if (ability is not GrappleAbilitySO grappleAbility)
        {
            Debug.LogError("GrappleHandler: Toggle must have GrappleAbilitySO passed in");
            return null;
        }

        _activeAbility = grappleAbility;

        PreCheckResult preCheckResult = PreCheck(_activeAbility);
        if (!preCheckResult.CanActivate)
        {
            EarlyRelease();
            return null;
        }

        _isGrappling = true;
        base.Toggle(ability);

        AbilityCooldownManager.UpdateCooldown(_activeAbility, _activeAbility.Cooldown);

        _grapplePoint = preCheckResult.TargetPoint?.transform.position ?? Vector3.zero;

        Debug.DrawLine(transform.position, _grapplePoint, Color.cyan, 2f);
        _inputHandler.FreezeInput(_activeAbility.DelayTime);
        _tongueController.ExtendTongue(_grapplePoint, _activeAbility.DelayTime);

        CancelInvoke(nameof(ExecuteGrapple));
        Invoke(nameof(ExecuteGrapple), _activeAbility.DelayTime);

        return new AbilityStartResult(true);
    }

    public void EarlyRelease()
    {
        _isGrappling = false;
        _inputHandler.FreezeInput(_activeAbility.DelayTime);
        Invoke(nameof(UntoggleGrapple), _activeAbility.DelayTime);
        base.Release();
    }

    public override void Release()
    {
        _isGrappling = false;

        CancelInvoke(nameof(ExecuteGrapple));
        CancelInvoke(nameof(UntoggleGrapple));

        _tongueController.AimTongue();
        float delayTime = _activeAbility ? _activeAbility.DelayTime : 0f;
        Invoke(nameof(UntoggleGrapple), delayTime);

        base.Release();
    }

    private void ExecuteGrapple()
    {
        _abilityUser.RequestMovementStateChange(new(MovementHandlerType.GRAPPLING, ability: _activeAbility, owner: _abilityUser.GetActiveAbility(), data: _grapplePoint));
    }

    public void UntoggleGrapple()
    {
        Debug.Log("GrappleHandler :: Unsuccessful grapple.");
    }
}
